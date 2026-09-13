using System.Text.Json.Serialization;
using CampanhasAPI.Api.Endpoints;
using CampanhasAPI.Api.Middleware;
using CampanhasAPI.IoC;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogConfiguration();

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter())); // enums como texto no JSON

builder.Services.AddJwtAuthenticationConfig(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddDomainServices();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddRabbitMQMessaging(builder.Configuration);
builder.Services.AddObservability();

var app = builder.Build();

try
{
    // Auto-migrate + seed do GestorONG e topologia do RabbitMQ (padrao da Fase 3)
    await app.MigrateAndSeedAsync();
    await app.InitializeRabbitMQAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha na inicializacao — verifique PostgreSQL e RabbitMQ");
    throw;
}

app.UseSerilogRequestLoggingConfiguration();
app.UseSwaggerDocumentation(); // sempre ligado: e a interface da demo
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseObservability();        // /metrics e /health

app.MapAuth();
app.MapDoadores();
app.MapCampanhas();
app.MapDoacoes();

app.Run();
