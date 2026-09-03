using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace SistemaDeGestaoComercial.Infraestrutura.Mensageria;

public sealed class RabbitMqConnection(IOptions<RabbitMqOptions> options) : IAsyncDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private IConnection? connection;

    public async Task<IConnection> ObterAsync(CancellationToken cancellationToken)
    {
        if (connection is { IsOpen: true })
            return connection;
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (connection is { IsOpen: true })
                return connection;
            if (connection is not null)
                await connection.DisposeAsync();
            var config = options.Value;
            var factory = new ConnectionFactory
            {
                HostName = config.HostName,
                Port = config.Port,
                UserName = config.UserName,
                Password = config.Password,
                VirtualHost = config.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
            };
            connection = await factory.CreateConnectionAsync("gestao-comercial-api", cancellationToken);
            return connection;
        }
        finally
        {
            gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
            await connection.DisposeAsync();
    }
}
