using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SistemaDeGestaoComercial.Infraestrutura.Persistencia;

namespace SistemaDeGestaoComercial.Infraestrutura.Mensageria;

public interface IEventPublisher
{
    Task PublicarAsync(OutboxMessage mensagem, CancellationToken cancellationToken);
}

public sealed class RabbitMqPublisher(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqPublisher> logger
) : IEventPublisher
{
    public async Task PublicarAsync(OutboxMessage mensagem, CancellationToken ct)
    {
        var conexao = await connection.ObterAsync(ct);
        await using var canal = await conexao.CreateChannelAsync(new CreateChannelOptions(true, true), ct);
        await RabbitMqTopologyInitializer.DeclararAsync(canal, options.Value, ct);
        var propriedades = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = mensagem.Tipo,
            MessageId = mensagem.Id.ToString(),
            CorrelationId = mensagem.CorrelationId,
            Timestamp = new AmqpTimestamp(new DateTimeOffset(mensagem.CreatedAt).ToUnixTimeSeconds()),
        };
        await canal.BasicPublishAsync(
            options.Value.Exchange,
            RabbitMqTopology.VendaRealizadaRoutingKey,
            mandatory: true,
            propriedades,
            Encoding.UTF8.GetBytes(mensagem.Conteudo),
            ct
        );
        logger.LogInformation(
            "Evento publicado {MessageId} {RoutingKey}",
            mensagem.Id,
            RabbitMqTopology.VendaRealizadaRoutingKey
        );
    }
}
