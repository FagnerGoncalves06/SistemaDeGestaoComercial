using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace SistemaDeGestaoComercial.Infraestrutura.Mensageria;

internal static class RabbitMqTopologyInitializer
{
    public static async Task DeclararAsync(IChannel canal, RabbitMqOptions options, CancellationToken ct)
    {
        await canal.ExchangeDeclareAsync(
            options.Exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: ct
        );
        await canal.ExchangeDeclareAsync(
            RabbitMqTopology.RetryExchange,
            ExchangeType.Direct,
            true,
            false,
            cancellationToken: ct
        );
        await canal.ExchangeDeclareAsync(
            RabbitMqTopology.DeadLetterExchange,
            ExchangeType.Direct,
            true,
            false,
            cancellationToken: ct
        );
        await canal.QueueDeclareAsync(RabbitMqTopology.EstoqueQueue, true, false, false, cancellationToken: ct);
        await canal.QueueBindAsync(
            RabbitMqTopology.EstoqueQueue,
            options.Exchange,
            RabbitMqTopology.VendaRealizadaRoutingKey,
            cancellationToken: ct
        );
        var retryArgs = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = options.RetryDelaySeconds * 1000,
            ["x-dead-letter-exchange"] = options.Exchange,
            ["x-dead-letter-routing-key"] = RabbitMqTopology.VendaRealizadaRoutingKey,
        };
        await canal.QueueDeclareAsync(
            RabbitMqTopology.RetryQueue,
            true,
            false,
            false,
            retryArgs,
            cancellationToken: ct
        );
        await canal.QueueBindAsync(
            RabbitMqTopology.RetryQueue,
            RabbitMqTopology.RetryExchange,
            RabbitMqTopology.RetryQueue,
            cancellationToken: ct
        );
        await canal.QueueDeclareAsync(RabbitMqTopology.DeadLetterQueue, true, false, false, cancellationToken: ct);
        await canal.QueueBindAsync(
            RabbitMqTopology.DeadLetterQueue,
            RabbitMqTopology.DeadLetterExchange,
            RabbitMqTopology.DeadLetterQueue,
            cancellationToken: ct
        );
    }
}
