# Conexão Solidária

MVP da plataforma de gestão de doadores e campanhas da ONG Esperança Solidária —
Hackathon 11NETT.

## Arquitetura

Dois microsserviços que se comunicam apenas por eventos no RabbitMQ:

| Serviço | Responsabilidade | Banco |
|---|---|---|
| **CampanhasAPI** | Autenticação (JWT / RBAC), cadastro de doadores, campanhas, recebimento de doações. Publica `DoacaoRecebidaEvent`. | PostgreSQL |
| **DoacoesWorker** | Consome a fila e atualiza o valor arrecadado da campanha. É o **único** componente que escreve esse valor. | MongoDB |

Cada serviço segue a mesma arquitetura em camadas
(`Api/Worker → IoC → Application → Domain ← Data, Messaging`) e é autocontido:
tem sua própria solution, `docker-compose.yml` (só com as dependências dele),
`dockerfile` e manifests em `k8s/`. Nenhum
compila, testa ou vira imagem dependendo do outro.

As decisões de arquitetura estão registradas em [`docs/decisoes/`](docs/decisoes/).

## Rodando localmente

Pré-requisitos: .NET 10 SDK, Docker Desktop.

```bash
# Opcao A — tudo em containers (e assim que a correcao deve rodar)
docker compose -f Orchestration/docker-compose.yml --profile app up -d --build
# CampanhasAPI: http://localhost:5001/swagger  ·  Painel: http://localhost:5002/publico/campanhas

# Opcao B — infra em containers, servicos no dotnet run (desenvolvimento)
# 1. Infraestrutura (RabbitMQ, PostgreSQL, MongoDB)
docker compose -f Orchestration/docker-compose.yml up -d

# 2. Build — cada serviço tem sua própria solution
dotnet build CampanhasAPI
dotnet build DoacoesWorker

# 3. Serviços, em terminais separados
dotnet run --project CampanhasAPI/src/CampanhasAPI.Api         # http://localhost:5080/swagger
dotnet run --project DoacoesWorker/src/DoacoesWorker.Worker   # http://localhost:5090
```

| Serviço | Endereço | Credenciais (dev) |
|---|---|---|
| RabbitMQ UI | http://localhost:15672 | dev / dev |
| PostgreSQL | localhost:5432 | dev / dev |
| MongoDB | localhost:27017 | dev / dev |

## Kubernetes (Docker Desktop)

Ligue o Kubernetes no Docker Desktop (Settings → Kubernetes → Enable) e:

```bash
./Orchestration/deploy-local.sh        # build das imagens com a tag do commit + apply + rollout
kubectl -n conexao-solidaria get pods   # 5 pods Running, 0 restarts
```

| Serviço | Endereço |
|---|---|
| CampanhasAPI (Swagger) | http://localhost:5001/swagger |
| Painel público (Worker) | http://localhost:5002/publico/campanhas |
| RabbitMQ UI | http://localhost:15672 |

Manifests: infra compartilhada em [`Orchestration/k8s/`](Orchestration/k8s/), cada serviço
em `<Servico>/k8s/`. Derrubar tudo: `kubectl delete -k Orchestration/k8s`.
Decisões (LoadBalancer vs NodePort, tags imutáveis, initContainers): [ADR 004](docs/decisoes/004-kubernetes-local.md).

## Observabilidade

Sobem junto com o `kubectl apply -k` (ou o `deploy-local.sh`):

| Ferramenta | Endereço | O que mostra |
|---|---|---|
| Grafana | http://localhost:3000 (admin / admin, ou anônimo) | dashboard **Conexão Solidária — Observabilidade**: req/s, latência p95, doações registradas × processadas, filas do RabbitMQ, CPU e memória por pod |
| Prometheus | http://localhost:9090/targets | descoberta automática dos pods, RabbitMQ e cAdvisor |
| `/health` | :5001 e :5002 | JSON com o status de cada dependência (Postgres/Mongo, RabbitMQ) |
| `/metrics` | :5001 e :5002 | formato Prometheus: HTTP, runtime .NET e métricas de negócio |

Decisões: [ADR 005](docs/decisoes/005-observabilidade.md).

> A seção de CI/CD será preenchida conforme as
> fases forem concluídas.

### Desenvolvendo um serviço isolado

Cada serviço tem um `docker-compose.yml` só com o que ele precisa:

```bash
cd CampanhasAPI && docker compose up -d     # PostgreSQL + RabbitMQ
cd DoacoesWorker && docker compose up -d    # MongoDB + RabbitMQ
```

> Os dois usam as mesmas portas do RabbitMQ — não suba ambos ao mesmo tempo.
> Para rodar a solução inteira, use `Orchestration/`.

## Fluxo de uma doação

```
Doador ──POST /api/doacoes──► CampanhasAPI ──DoacaoRecebidaEvent──► RabbitMQ ──► DoacoesWorker ──$inc──► MongoDB
                                  │ grava Doacao (Pendente → Publicada)                │ ledger de idempotência
                                  │ NÃO soma nada                                       ▼
                                  ▼                                        GET /publico/campanhas
                              202 Accepted                                 (Título, Meta, ValorTotalArrecadado)
```

| Papel | Endereço | Credenciais (dev) |
|---|---|---|
| CampanhasAPI (Swagger) | http://localhost:5080/swagger | gestor@esperancasolidaria.org / Gestor@123 |
| DoacoesWorker — painel público | http://localhost:5090/publico/campanhas | — |
| RabbitMQ UI | http://localhost:15672 | dev / dev |
| PostgreSQL | localhost:5432 | dev / dev |
| MongoDB | localhost:27017 | dev / dev |
