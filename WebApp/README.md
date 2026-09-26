# WebApp — front-end experimental (Blazor)

> **Fora do escopo do MVP entregue no Hackathon 11NETT.** O enunciado pede apenas
> back-end; este front existe como referência pessoal para projetos futuros. Não é
> citado no vídeo, no relatório nem nos entregáveis, e não faz parte dos manifests do
> Kubernetes; no `docker-compose` fica num profile próprio (`front`), fora do padrão.

Front em **Blazor Web App (modo Auto)** consumindo o **API Gateway** da Conexão Solidária.

## Como rodar

```bash
# 1. back-end de pé (raiz do repositório)
docker compose -f Orchestration/docker-compose.yml --profile app up -d

# 2. front
dotnet run --project WebApp/src/WebApp     # http://localhost:5100
```

Ou tudo em Docker (back-end + front):

```bash
docker compose -f Orchestration/docker-compose.yml --profile app --profile front up -d --build
```

No container, o servidor do Blazor chama `http://api-gateway:5000` (variável
`Api__BaseUrl`): no modo Auto o 1º acesso roda no servidor, onde `localhost` é o próprio
container. O WebAssembly, no navegador, continua em `http://localhost:5000`.

A URL da API fica em `src/WebApp/wwwroot/appsettings.json` (lida pelo WebAssembly) e em
`src/WebApp/appsettings.json` (lida pelo servidor). Padrão: `http://localhost:5000`.

## Telas

| Rota | Quem acessa | Requisito |
|---|---|---|
| `/` | público | RF-4 — painel de transparência (dados do DoacoesWorker) |
| `/cadastro` | público | RF-3 — cadastro de doador |
| `/login` | público | RF-1 — autenticação, JWT no `localStorage` |
| `/campanhas`, `/campanhas/nova`, `/campanhas/{id}` | GestorONG | RF-2 — CRUD e mudança de status |
| `/doar/{id}` | Doador | RF-5 — doação (202 Accepted) |
| `/minhas-doacoes` | Doador | histórico |

## Estrutura

```
src/WebApp/          host ASP.NET — serve o WebAssembly e renderiza o 1º acesso
src/WebApp.Client/   o que roda no navegador
  Models/            DTOs espelhando a API
  Services/          TokenStore · ApiClient · AuthService
  Auth/              JwtAuthenticationStateProvider
  Layout/            AppLayout (menu reage à role)
  Pages/             as 7 telas
```

## Decisões que valem lembrar

**Modo Auto = os serviços são registrados duas vezes.** O primeiro acesso renderiza no
servidor (SignalR); depois o WebAssembly assume. Os dois lados precisam do mesmo
container de injeção de dependência — daí `ServicosDoWebApp.AddWebAppServices`, chamado
nos dois `Program.cs`.

**Interatividade global com prerender ligado** (`App.razor`). O `localStorage` não existe
no servidor, então o `TokenStore` devolve "sem token" nessa janela e o `AppLayout`
reavalia a sessão em `OnAfterRenderAsync`, quando o navegador assume.

**`Routes.razor` mora no `.Client`, não no host.** Com interatividade global, ele é o
componente raiz — e o WebAssembly só recebe o assembly `WebApp.Client`. No host, o 1º
acesso (servidor) funciona e o erro só aparece quando o navegador assume:
`Root component type 'WebApp.Components.Routes' could not be found`.

**`MapRazorComponents(...).AllowAnonymous()` no servidor.** Como o servidor nunca vê o JWT,
abrir por URL (ou F5) uma página com `[Authorize]` dava 500: o middleware de autorização
tentava um challenge sem esquema de autenticação. A checagem fica no `AuthorizeRouteView`.

**Dockerfile sem `--no-restore` no publish.** O pacote que traz o `blazor.web.js` só entra
no restore quando o SDK enxerga os `.razor`; restaurando só com os `.csproj`, a página abre
sem interatividade (botões mortos, `blazor.web.js` 404).

**O JWT não é validado no cliente.** Validar assinatura no navegador não protege nada — o
usuário controla o navegador. As claims servem só para a interface (mostrar o nome,
esconder o menu do gestor). A autorização real está na API, que devolve 401/403.

**Token no `localStorage`.** Simples e suficiente aqui. Em produção, considere cookie
`HttpOnly`: `localStorage` é legível por qualquer script da página (risco de XSS).

**CORS é do back-end, não do front.** O navegador bloqueia a resposta se o servidor não
autorizar a origem. Foi preciso habilitar no **gateway** (é com ele que o front fala) — e
lá o middleware precisa vir antes do Ocelot, porque o preflight `OPTIONS` não consta nas
rotas do `ocelot.json` e viraria 404.

**Paleta** gerada a partir do índigo `#3D438F` (Radix custom), com variante escura
automática via `prefers-color-scheme`.
