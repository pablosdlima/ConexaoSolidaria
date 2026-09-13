using DoacoesWorker.IoC;
using DoacoesWorker.Worker.Endpoints;
using Serilog;

// O Worker e um host de BackgroundServices (os consumers) com um servidor HTTP
// minimo por cima: painel publico (ADR 003), /health (probes) e /metrics (Prometheus).
var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogConfiguration();

builder.Services.AddSwaggerDocumentation();
builder.Services.AddMongo(builder.Configuration);
builder.Services.AddDomainServices();
builder.Services.AddApplicationServices();
builder.Services.AddRabbitMQMessaging(builder.Configuration);
builder.Services.AddObservability();

var app = builder.Build();

try
{
    await app.InitializeMongoAsync();
    await app.InitializeRabbitMQAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha na inicializacao — verifique MongoDB e RabbitMQ");
    throw;
}

app.UseSerilogRequestLoggingConfiguration();
app.UseSwaggerDocumentation();
app.UseObservability();   // /metrics e /health
app.MapPainel();

app.Run();
