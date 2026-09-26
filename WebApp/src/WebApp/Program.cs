using WebApp.Client;
using WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Os mesmos servicos do lado WebAssembly (ver ServicosDoWebApp): no modo Auto
// o primeiro acesso renderiza aqui, no servidor.
builder.Services.AddWebAppServices(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5000");

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();
else
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(WebApp.Client._Imports).Assembly)
    // O JWT vive no localStorage: o servidor nunca sabe quem esta logado. Sem isto, abrir
    // uma pagina com [Authorize] pela URL (ou F5) da 500 — o middleware de autorizacao tenta
    // um challenge sem esquema de autenticacao. Quem decide e o AuthorizeRouteView
    // (interface) e, de verdade, a API (401/403).
    .AllowAnonymous();

app.Run();
