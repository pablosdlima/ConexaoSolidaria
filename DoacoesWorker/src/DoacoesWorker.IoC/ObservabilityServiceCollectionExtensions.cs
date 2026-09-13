using System.Text.Json;
using DoacoesWorker.Domain.Interfaces.Services;
using DoacoesWorker.IoC.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace DoacoesWorker.IoC;

public static class ObservabilityServiceCollectionExtensions
{
    /// <summary>Health checks reais (banco e broker) e metricas de negocio.</summary>
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services.AddSingleton<IMetricas, PrometheusMetricas>();

        services.AddHealthChecks()
            .AddCheck<MongoHealthCheck>("mongo", tags: ["ready"])
            .AddCheck<RabbitMQHealthCheck>("rabbitmq", tags: ["ready"]);

        return services;
    }

    /// <summary>
    /// /metrics  → formato Prometheus (HTTP + .NET runtime + negocio)
    /// /health   → JSON com o status de cada dependencia (readiness e liveness do Kubernetes)
    /// </summary>
    public static WebApplication UseObservability(this WebApplication app)
    {
        app.Services.GetRequiredService<IMetricas>(); // registra os contadores de negocio ja no startup (o dashboard mostra 0, nao "No data")
        app.UseHttpMetrics();
        app.MapMetrics("/metrics");
        app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = EscreverJson });
        return app;
    }

    private static Task EscreverJson(HttpContext ctx, HealthReport report)
    {
        ctx.Response.ContentType = "application/json";
        var corpo = new
        {
            status = report.Status.ToString(),
            duracaoMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            checks = report.Entries.Select(e => new { nome = e.Key, status = e.Value.Status.ToString(), descricao = e.Value.Description })
        };
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(corpo));
    }
}
