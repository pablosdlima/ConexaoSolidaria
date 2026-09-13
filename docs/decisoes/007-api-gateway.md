# ADR 007 — API Gateway (Ocelot)

**Status:** aceita
**Data:** 2026-09-13

## Contexto

Bônus do enunciado: "utilização de um API Gateway (ex.: Ocelot, KrakenD) roteando
as requisições para os microsserviços". Sem impacto na nota, mas resolve um
incômodo real do desenho: o cliente precisava conhecer dois endereços — a API
(`:5001`) e o painel público no Worker (`:5002`, ADR 003).

## Decisão

**Ocelot**, como terceiro componente em `ApiGateway/`, com o mesmo tratamento dos
outros (solution, `dockerfile`, `k8s/`, job no CI, imagem no GHCR) — mas **sem
camadas**: gateway é infraestrutura de borda, não tem domínio. É um `Program.cs`
e um `ocelot.json`.

**Um endereço para o cliente:** `http://localhost:5000/api/...`

| Upstream (cliente vê) | Downstream (quem atende) |
|---|---|
| `/api/auth`, `/api/doadores`, `/api/campanhas`, `/api/doacoes` | `campanhas-api:8080` |
| `/api/publico/campanhas` | `doacoes-worker:8080/publico/campanhas` |

O painel público continua vivendo no Worker (ADR 003), mas para quem chama é só
mais uma rota `/api`. A topologia interna deixa de vazar para o cliente.

**Rate limit no login:** 10 tentativas por minuto por cliente, HTTP 429 acima disso.
Proteção contra força bruta é responsabilidade de borda — um lugar, para todos os
serviços — e não da API.

**Ocelot só em `/api/*`.** O middleware do Ocelot é terminal: sem o `MapWhen`, ele
respondia 404 para `/health` e as probes do Kubernetes reiniciavam o pod em loop
(descoberto no primeiro deploy no cluster — no compose, sem probes, passou despercebido).

**O gateway não valida JWT.** Cada serviço é dono da própria autorização (a API
já valida token e role). Validar também no gateway exigiria compartilhar a chave e
duplicar regra; o ganho seria rejeitar um pouco antes. Fica como evolução.

**Hosts por ambiente:** `ocelot.json` usa os nomes dos Services/containers
(`campanhas-api`, `doacoes-worker`), válidos no Kubernetes e no compose;
`ocelot.Development.json` sobrescreve para `localhost:5080/5090` no `dotnet run`.
No cluster o gateway roda como `Production` para usar só o primeiro. Os Services
da API e do Worker ganharam a porta interna `8080` para o gateway.

## Alternativas descartadas

- **KrakenD.** Só configuração, sem código — mais leve. Mas é Go e um formato
  próprio; Ocelot fica na stack .NET e o time consegue depurar.
- **Kong (usado na Fase 3).** Muito mais completo (plugins, admin API), e por isso
  mais pesado no cluster local — mais um Postgres só para ele, ou modo DB-less com
  configuração declarativa extensa. Desproporcional para 5 rotas.
- **Ingress Controller.** Faria o roteamento por path, mas não o rate limit por
  rota nem a reescrita `/api/publico → /publico` sem anotações específicas do
  controller. E é mais uma instalação no cluster.
