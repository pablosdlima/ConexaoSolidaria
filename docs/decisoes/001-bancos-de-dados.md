# ADR 001 — Escolha dos bancos de dados

**Status:** aceita
**Data:** 2026-09-11

## Contexto

O entregável 2b pede um PDF justificando "por que os bancos de dados X e Y foram
escolhidos". A frase pressupõe dois bancos. Os requisitos técnicos não tornam isso
explícito, mas a arquitetura em microsserviços com mensageria já cria dois
contextos de dados com necessidades diferentes:

| Contexto | Quem escreve | Natureza | Padrão de acesso |
|---|---|---|---|
| Usuários, campanhas, doações | API | Transacional, relacional, com integridade (e-mail único, FK doação→campanha) | Escrita + leitura autenticada |
| Valor arrecadado por campanha | Worker | Agregação derivada de eventos | Escrita frequente pelo Worker, leitura pública de alto volume |

## Decisão

- **PostgreSQL** para o contexto transacional da API.
- **MongoDB** para o modelo de leitura mantido pelo Worker.

### Por que PostgreSQL para a API

- Integridade que o enunciado exige: índice único em `Email`, chave estrangeira de
  `Doacao` para `Campanha`, `decimal` com precisão exata para `MetaFinanceira` e
  `ValorDoacao` (dinheiro nunca em ponto flutuante).
- Transação ACID no fluxo "persistir doação como pendente + publicar evento".
- Suporte maduro no EF Core (Npgsql), migrations versionadas no repositório.
- Imagem oficial leve, roda bem em Minikube/Kind com um único volume.

### Por que MongoDB para o Worker

- O Worker mantém um documento por campanha (`campanhas_resumo`) com título, meta,
  status e total arrecadado — exatamente o formato que o painel público devolve.
  O endpoint público lê um documento pronto, sem join e sem tocar no banco
  transacional.
- O ledger de idempotência (`doacoes_processadas`, indexado por `IdDoacao`) é
  append-only e não precisa de relacionamentos.
- Separar o banco do Worker do banco da API respeita *database per service*:
  cada serviço é dono do seu dado e o acoplamento entre eles é só o evento.

### Consequências

- O Worker precisa saber título/meta/status da campanha para montar o resumo.
  A API publica também um `CampanhaAtualizadaEvent` ao criar/editar campanha —
  uma segunda mensagem, mesma infraestrutura.
- Consistência eventual no painel público: o valor aparece segundos depois da
  doação. É o comportamento esperado do enunciado (o Worker atualiza, não a API).
- Dois bancos no cluster em vez de um: mais dois manifests e dois volumes.

## Alternativas descartadas

**Um único PostgreSQL com duas databases (`api_db`, `worker_db`).** Atende
*database per service* e é mais simples de operar, mas não sustenta a
justificativa "X e Y" que o entregável pede. Fica como fallback se o Discord
confirmar que um banco só é aceitável.

**SQL Server no lugar do PostgreSQL.** Foi o banco relacional da Fase 3 e o time já
conhece. Descartado pelo ambiente de execução: a imagem `mssql/server` tem ~1,5 GB e
exige 2 GB de RAM só para subir — num cluster local (Minikube/Kind) disputando
memória com RabbitMQ, MongoDB, Prometheus e Grafana, isso derruba pods. O
`postgres:17` tem ~150 MB e sobe com 64 MB. Para o EF Core a troca é só o provider
(`Npgsql` em vez de `SqlServer`); o resto do código é idêntico.

**Um único banco para os dois serviços.** Mais simples, mas fere *database per
service*: o Worker passaria a escrever na tabela da API, e os dois ficariam
acoplados pelo schema em vez de pelo evento. Perde justamente o argumento
arquitetural que o hackathon avalia.
