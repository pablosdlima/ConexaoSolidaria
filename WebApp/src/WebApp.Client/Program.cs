using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// A URL da API vem de wwwroot/appsettings.json do projeto host — assim da para
// trocar o endereco sem recompilar o WebAssembly.
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5000";
builder.Services.AddWebAppServices(apiBaseUrl);

await builder.Build().RunAsync();
