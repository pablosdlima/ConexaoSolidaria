using DoacoesWorker.IoC;
using DoacoesWorker.Worker.Endpoints;

// O Worker e um host de BackgroundServices (os consumers) com um servidor HTTP
// minimo por cima: painel publico (ADR 003), /health (probes) e, na Fase 7, /metrics.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerDocumentation();
builder.Services.AddMongo(builder.Configuration);
builder.Services.AddDomainServices();
builder.Services.AddApplicationServices();
builder.Services.AddRabbitMQMessaging(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

await app.InitializeMongoAsync();
await app.InitializeRabbitMQAsync();

app.UseSwaggerDocumentation();
app.MapHealthChecks("/health");
app.MapPainel();

app.Run();
