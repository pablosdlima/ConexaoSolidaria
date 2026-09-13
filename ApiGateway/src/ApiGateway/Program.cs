using System.Threading.RateLimiting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

// API Gateway (Ocelot): ponto unico de entrada. Roteia para a CampanhasAPI e para o
// painel publico do DoacoesWorker. Nao valida JWT: cada servico e dono da propria
// autorizacao (ADR 007). Rate limit por IP no login, com o middleware do ASP.NET.
var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddHealthChecks();

// Forca bruta no login: 10 tentativas por minuto por IP. As demais rotas nao sao limitadas.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        ctx.Request.Path.StartsWithSegments("/api/auth")
            ? RateLimitPartition.GetFixedWindowLimiter(
                "login:" + (ctx.Connection.RemoteIpAddress?.ToString() ?? "desconhecido"),
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) })
            : RateLimitPartition.GetNoLimiter("sem-limite"));
});

var app = builder.Build();

app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new
{
    servico = "Conexao Solidaria - API Gateway",
    rotas = new[] { "/api/auth", "/api/doadores", "/api/campanhas", "/api/doacoes", "/api/publico/campanhas" }
}));

// Ocelot e terminal: sem este filtro ele responderia 404 para /health e as probes do Kubernetes
// reiniciariam o pod. So /api/* passa pelo gateway; /health e / ficam com o ASP.NET.
app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/api"), b => b.UseOcelot().Wait());
app.Run();
