using CampanhasAPI.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CampanhasAPI.IoC.HealthChecks;

public class RabbitMQHealthCheck(RabbitMQConexao conexao) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            var conn = await conexao.ObterAsync(ct);
            return conn.IsOpen
                ? HealthCheckResult.Healthy("RabbitMQ conectado")
                : HealthCheckResult.Unhealthy("Conexao com o RabbitMQ fechada");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ indisponivel", ex);
        }
    }
}
