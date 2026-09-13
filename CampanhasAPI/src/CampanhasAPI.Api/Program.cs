using System.Text.Json.Serialization;
using CampanhasAPI.Api.Endpoints;
using CampanhasAPI.Api.Middleware;
using CampanhasAPI.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter())); // enums como texto no JSON

builder.Services.AddJwtAuthenticationConfig(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddDomainServices();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddRabbitMQMessaging(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Auto-migrate + seed do GestorONG (padrao da Fase 3)
await app.MigrateAndSeedAsync();
await app.InitializeRabbitMQAsync();

app.UseSwaggerDocumentation(); // sempre ligado: e a interface da demo
app.MapHealthChecks("/health");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuth();
app.MapDoadores();
app.MapCampanhas();
app.MapDoacoes();

app.Run();
