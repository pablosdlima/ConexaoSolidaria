using DoacoesWorker.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DoacoesWorker.IoC.HealthChecks;

public class MongoHealthCheck(MongoDbContext ctx) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await ctx.PingAsync(ct);
            return HealthCheckResult.Healthy("MongoDB acessivel");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MongoDB indisponivel", ex);
        }
    }
}
