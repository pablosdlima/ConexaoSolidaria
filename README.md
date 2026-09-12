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
# 1. Infraestrutura completa (RabbitMQ, PostgreSQL, MongoDB)
docker compose -f Orchestration/docker-compose.yml up -d

# 2. Build — cada serviço tem sua própria solution
dotnet build CampanhasAPI
dotnet build DoacoesWorker

# 3. Serviços, em terminais separados
dotnet run --project CampanhasAPI/src/CampanhasAPI.Api
dotnet run --project DoacoesWorker/src/DoacoesWorker.Worker
```

| Serviço | Endereço | Credenciais (dev) |
|---|---|---|
| RabbitMQ UI | http://localhost:15672 | dev / dev |
| PostgreSQL | localhost:5432 | dev / dev |
| MongoDB | localhost:27017 | dev / dev |

> As seções de Kubernetes, observabilidade e CI/CD serão preenchidas conforme as
> fases forem concluídas.

### Desenvolvendo um serviço isolado

Cada serviço tem um `docker-compose.yml` só com o que ele precisa:

```bash
cd CampanhasAPI && docker compose up -d     # PostgreSQL + RabbitMQ
cd DoacoesWorker && docker compose up -d    # MongoDB + RabbitMQ
```

> Os dois usam as mesmas portas do RabbitMQ — não suba ambos ao mesmo tempo.
> Para rodar a solução inteira, use `Orchestration/`.
