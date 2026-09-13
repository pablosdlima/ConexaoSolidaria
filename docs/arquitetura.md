# Arquitetura — Conexão Solidária

Dois microsserviços que se comunicam **apenas por eventos**, cada um dono do próprio
banco. Um API Gateway na borda. Observabilidade com Prometheus e Grafana. Tudo no
Kubernetes, entregue por um pipeline que gera as imagens a cada push.

```mermaid
flowchart LR
    subgraph cliente[" "]
        U["Doador · GestorONG<br/><i>Postman / Swagger</i>"]
    end

    subgraph k8s["Kubernetes · namespace conexao-solidaria"]
        direction LR
        GW["<b>API Gateway</b><br/>Ocelot · :5000<br/><i>rate limit no login</i>"]

        subgraph api["CampanhasAPI"]
            A["<b>ASP.NET · JWT / RBAC</b><br/>/api/auth · /api/doadores<br/>/api/campanhas · /api/doacoes"]
            PG[("<b>PostgreSQL</b><br/>usuarios · campanhas · doacoes")]
        end

        MQ{{"<b>RabbitMQ</b><br/>exchange conexao-solidaria<br/>filas + DLQ"}}

        subgraph worker["DoacoesWorker"]
            W["<b>BackgroundServices</b><br/>consumers idempotentes<br/>GET /publico/campanhas"]
            MG[("<b>MongoDB</b><br/>campanhas_resumo<br/>doacoes_processadas")]
        end

        subgraph obs["Observabilidade"]
            P["Prometheus"]
            G["Grafana"]
        end
    end

    U -->|"HTTP"| GW
    GW -->|"/api/*"| A
    GW -->|"/api/publico → /publico"| W
    A --> PG
    A -->|"DoacaoRecebidaEvent<br/>CampanhaAtualizadaEvent"| MQ
    MQ -->|"doacoes-recebidas<br/>campanhas-atualizadas"| W
    W -->|"$inc · upsert"| MG
    P -.->|"/metrics"| A
    P -.->|"/metrics"| W
    P -.->|":15692"| MQ
    G --> P

    classDef svc fill:#DCEEEA,stroke:#0B7A6A,color:#121A18
    classDef db fill:#F6F7F5,stroke:#61706C,color:#121A18
    classDef mq fill:#FFF4E5,stroke:#A84B18,color:#121A18
    classDef obsc fill:#EEF0FF,stroke:#4A56A6,color:#121A18
    class GW,A,W svc
    class PG,MG db
    class MQ mq
    class P,G obsc
```

## O fluxo de uma doação

```mermaid
sequenceDiagram
    autonumber
    actor D as Doador
    participant GW as API Gateway
    participant API as CampanhasAPI
    participant PG as PostgreSQL
    participant MQ as RabbitMQ
    participant W as DoacoesWorker
    participant MG as MongoDB

    D->>GW: POST /api/doacoes {IdCampanha, ValorDoacao} + JWT
    GW->>API: encaminha
    API->>PG: campanha existe? aceita doações?
    API->>PG: grava Doacao (Pendente)
    API->>MQ: publica DoacaoRecebidaEvent
    API->>PG: marca Publicada
    API-->>D: 202 Accepted
    Note over API: a API NÃO soma nada

    MQ->>W: entrega doacoes-recebidas
    W->>MG: insere no ledger (chave = IdDoacao)
    alt já existia (reentrega)
        W-->>MQ: ACK — ignora
    else primeira vez
        W->>MG: $inc ValorTotalArrecadado
        W-->>MQ: ACK
    end

    D->>GW: GET /api/publico/campanhas
    GW->>W: /publico/campanhas
    W->>MG: campanhas com Status = Ativa
    W-->>D: Titulo, MetaFinanceira, ValorTotalArrecadado
```

## Entrega e observabilidade

```mermaid
flowchart LR
    DEV["git push<br/>main"] --> CI["GitHub Actions<br/>3 jobs: build · test · docker"]
    CI --> GHCR["GHCR<br/>campanhas-api · doacoes-worker · api-gateway<br/>tag = SHA do commit"]
    GHCR -.->|"pull"| K8S["Kubernetes<br/>kubectl apply -k Orchestration/k8s"]
    K8S --> GRAF["Grafana<br/>req/s · p95 · filas · CPU · memória<br/>doações registradas × processadas"]
```

## Decisões

Cada seta acima tem um ADR: [`docs/decisoes/`](decisoes/).

| Tema | ADR |
|---|---|
| PostgreSQL + MongoDB, um por serviço | [001](decisoes/001-bancos-de-dados.md) |
| Monorepo com serviços autocontidos | [002](decisoes/002-estrutura-do-repositorio.md) |
| Painel público no Worker (CQRS) | [003](decisoes/003-painel-de-transparencia.md) |
| Kubernetes local, LoadBalancer, tags imutáveis | [004](decisoes/004-kubernetes-local.md) |
| Prometheus, Grafana, métricas de negócio | [005](decisoes/005-observabilidade.md) |
| SOLID e repositório/serviço genéricos | [006](decisoes/006-solid-e-repositorio-generico.md) |
| API Gateway (Ocelot) | [007](decisoes/007-api-gateway.md) |
