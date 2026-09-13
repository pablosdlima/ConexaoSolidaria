using CampanhasAPI.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CampanhasAPI.IoC.HealthChecks;

public class PostgresHealthCheck(CampanhasDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(ct)
                ? HealthCheckResult.Healthy("PostgreSQL acessivel")
                : HealthCheckResult.Unhealthy("PostgreSQL nao respondeu");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL indisponivel", ex);
        }
    }
}
