using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

namespace DoacoesWorker.IoC;

/// <summary>Adaptado da Fase 3: configuracao vem da secao "Serilog" do appsettings.</summary>
public static class SerilogServiceCollectionExtensions
{
    public static void AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "DoacoesWorker")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    public static void UseSerilogRequestLoggingConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging(o =>
        {
            o.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0} ms";
            o.GetLevel = (ctx, _, ex) => ex is not null || ctx.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : ctx.Request.Path.StartsWithSegments("/health") || ctx.Request.Path.StartsWithSegments("/metrics")
                    ? LogEventLevel.Debug   // probes e scrapes nao poluem o log
                    : LogEventLevel.Information;
            o.EnrichDiagnosticContext = (diag, ctx) =>
            {
                diag.Set("RemoteIP", ctx.Connection.RemoteIpAddress?.ToString());
                diag.Set("CorrelationId", ctx.TraceIdentifier);
                diag.Set("Usuario", ctx.User.Identity?.IsAuthenticated == true ? ctx.User.FindFirst("email")?.Value : null);
            };
        });
    }
}
