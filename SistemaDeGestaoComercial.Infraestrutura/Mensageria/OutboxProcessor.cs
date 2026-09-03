using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SistemaDeGestaoComercial.Infraestrutura.Persistencia;

namespace SistemaDeGestaoComercial.Infraestrutura.Mensageria;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<OutboxProcessor> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
            return;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(1, options.Value.PollingIntervalSeconds)));
        do
        {
            try
            {
                await ProcessarAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha no ciclo do processador Outbox");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessarAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var mensagens = await contexto
            .OutboxMessages.Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.CreatedAt)
            .Take(options.Value.BatchSize)
            .ToListAsync(ct);
        var processadas = 0;
        foreach (var mensagem in mensagens)
        {
            try
            {
                await publisher.PublicarAsync(mensagem, ct);
                mensagem.MarcarProcessada();
                processadas++;
                logger.LogInformation("Mensagem Outbox publicada {MessageId}", mensagem.Id);
            }
            catch (Exception ex)
            {
                mensagem.RegistrarFalha(ex.Message);
                logger.LogWarning(
                    ex,
                    "Falha ao publicar mensagem Outbox {MessageId} tentativa {Tentativa}",
                    mensagem.Id,
                    mensagem.Tentativas
                );
            }
            await contexto.SaveChangesAsync(ct);
        }
        if (mensagens.Count > 0)
            logger.LogInformation("Quantidade de mensagens Outbox processadas {Quantidade}", processadas);
    }
}
