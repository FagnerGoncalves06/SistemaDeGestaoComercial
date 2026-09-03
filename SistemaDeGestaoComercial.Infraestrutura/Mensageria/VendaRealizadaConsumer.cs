using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Aplicacao.Contratos;
using SistemaDeGestaoComercial.Dominio.Entidades;
using SistemaDeGestaoComercial.Infraestrutura.Persistencia;

namespace SistemaDeGestaoComercial.Infraestrutura.Mensageria;

public sealed class VendaRealizadaConsumer(
    RabbitMqConnection connection,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<VendaRealizadaConsumer> logger
) : BackgroundService
{
    private IChannel? canal;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
            return;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var conexao = await connection.ObterAsync(stoppingToken);
                canal = await conexao.CreateChannelAsync(new CreateChannelOptions(true, true), stoppingToken);
                await RabbitMqTopologyInitializer.DeclararAsync(canal, options.Value, stoppingToken);
                await canal.BasicQosAsync(0, 10, false, stoppingToken);
                var consumer = new AsyncEventingBasicConsumer(canal);
                consumer.ReceivedAsync += ProcessarAsync;
                await canal.BasicConsumeAsync(RabbitMqTopology.EstoqueQueue, false, consumer, stoppingToken);
                logger.LogInformation("Consumidor iniciado na fila {Queue}", RabbitMqTopology.EstoqueQueue);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;
                logger.LogError(ex, "Consumidor RabbitMQ indisponível; nova tentativa em 5 segundos");
                if (canal is not null)
                {
                    await canal.DisposeAsync();
                    canal = null;
                }
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private async Task ProcessarAsync(object sender, BasicDeliverEventArgs args)
    {
        if (canal is null)
            return;
        var body = args.Body.ToArray();
        var cancellationToken = args.CancellationToken;
        try
        {
            var evento =
                JsonSerializer.Deserialize<VendaRealizadaEvent>(body)
                ?? throw new JsonException("Evento VendaRealizada inválido.");
            logger.LogInformation(
                "Evento recebido {EventoId} {VendaId} {NumeroVenda}",
                evento.EventoId,
                evento.VendaId,
                evento.NumeroVenda
            );
            await using var scope = scopeFactory.CreateAsyncScope();
            var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inbox = scope.ServiceProvider.GetRequiredService<IInboxRepositorio>();
            await using var transacao = await contexto.Database.BeginTransactionAsync(cancellationToken);
            if (await inbox.JaProcessadaAsync(evento.EventoId, RabbitMqTopology.ConsumerName, cancellationToken))
            {
                logger.LogInformation("Evento ignorado por idempotência {EventoId}", evento.EventoId);
            }
            else
            {
                var ids = evento.Itens.Select(x => x.ProdutoId).Distinct().ToArray();
                var produtos = await contexto.Produtos.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
                foreach (var produto in produtos)
                {
                    var alerta = AlertaEstoque.CriarSeEstoqueBaixo(produto, evento.VendaId, evento.NumeroVenda);
                    if (alerta is null)
                        continue;
                    contexto.AlertasEstoque.Add(alerta);
                    logger.LogInformation(
                        "Alerta de estoque criado {EventoId} {ProdutoId} {QuantidadeAtual}",
                        evento.EventoId,
                        produto.Id,
                        produto.QuantidadeEstoque
                    );
                }
                inbox.Adicionar(evento.EventoId, RabbitMqTopology.ConsumerName);
                await contexto.SaveChangesAsync(cancellationToken);
                await transacao.CommitAsync(cancellationToken);
            }
            await canal.BasicAckAsync(args.DeliveryTag, false, cancellationToken);
        }
        catch (Exception ex)
        {
            await EncaminharFalhaAsync(args, body, ex, cancellationToken);
        }
    }

    private async Task EncaminharFalhaAsync(
        BasicDeliverEventArgs args,
        byte[] body,
        Exception ex,
        CancellationToken cancellationToken
    )
    {
        if (canal is null)
            return;
        var tentativa = ObterTentativa(args.BasicProperties.Headers) + 1;
        var propriedades = new BasicProperties
        {
            Persistent = true,
            ContentType = args.BasicProperties.ContentType,
            Type = args.BasicProperties.Type,
            MessageId = args.BasicProperties.MessageId,
            CorrelationId = args.BasicProperties.CorrelationId,
            Headers = new Dictionary<string, object?>
            {
                ["x-retry-count"] = tentativa,
                ["x-last-error"] = Encoding.UTF8.GetBytes(ex.Message[..Math.Min(ex.Message.Length, 500)]),
            },
        };
        if (tentativa <= options.Value.MaxRetries)
        {
            await canal.BasicPublishAsync(
                RabbitMqTopology.RetryExchange,
                RabbitMqTopology.RetryQueue,
                true,
                propriedades,
                body,
                cancellationToken
            );
            logger.LogWarning(
                ex,
                "Retry realizado {MessageId} tentativa {Tentativa}",
                propriedades.MessageId,
                tentativa
            );
        }
        else
        {
            await canal.BasicPublishAsync(
                RabbitMqTopology.DeadLetterExchange,
                RabbitMqTopology.DeadLetterQueue,
                true,
                propriedades,
                body,
                cancellationToken
            );
            logger.LogError(
                ex,
                "Mensagem enviada para DLQ {MessageId} após {Tentativa} tentativas",
                propriedades.MessageId,
                tentativa - 1
            );
        }
        await canal.BasicAckAsync(args.DeliveryTag, false, cancellationToken);
    }

    private static int ObterTentativa(IDictionary<string, object?>? headers)
    {
        if (headers is null || !headers.TryGetValue("x-retry-count", out var value))
            return 0;
        return value switch
        {
            int n => n,
            long n => checked((int)n),
            byte[] b when int.TryParse(Encoding.UTF8.GetString(b), out var n) => n,
            _ => 0,
        };
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (canal is not null)
            await canal.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
