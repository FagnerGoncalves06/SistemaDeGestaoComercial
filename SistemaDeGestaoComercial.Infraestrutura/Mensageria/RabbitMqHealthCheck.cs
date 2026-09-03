using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace SistemaDeGestaoComercial.Infraestrutura.Mensageria;

public sealed class RabbitMqHealthCheck(RabbitMqConnection connection, IOptions<RabbitMqOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        if (!options.Value.Enabled)
            return HealthCheckResult.Healthy("RabbitMQ desabilitado por configuração.");
        try
        {
            return (await connection.ObterAsync(ct)).IsOpen
                ? HealthCheckResult.Healthy("RabbitMQ acessível.")
                : HealthCheckResult.Degraded("RabbitMQ desconectado.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("RabbitMQ indisponível; vendas continuam protegidas pela Outbox.", ex);
        }
    }
}
