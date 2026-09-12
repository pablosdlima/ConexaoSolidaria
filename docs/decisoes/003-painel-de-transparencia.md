# ADR 003 — Quem serve o painel de transparência

**Status:** aceita
**Data:** 2026-09-12

## Contexto

O RF-4 pede uma API pública que liste campanhas ativas com título, meta e
**valor total arrecadado**. O RT de mensageria diz que a API **não** atualiza esse
valor — quem atualiza é o Worker. E o ADR 001 dá a cada serviço o seu banco.

Juntando os três: o valor arrecadado nasce no Worker e mora no MongoDB dele.
Se a `CampanhasAPI` servisse o painel, teria que ler o banco do Worker —
exatamente o acoplamento que *database per service* proíbe.

## Decisão

**O painel público é servido pelo `DoacoesWorker`**, porque ele é o dono do dado.

- A `Campanha` da API **não tem** `ValorTotalArrecadado`. O campo simplesmente
  não existe no PostgreSQL. É impossível a API escrevê-lo "sem querer".
- A API publica `CampanhaAtualizadaEvent` (título, meta, status, datas) ao
  criar, editar ou mudar o status de uma campanha.
- O Worker mantém `campanhas_resumo` no MongoDB com esses campos mais o total,
  e expõe `GET /publico/campanhas` filtrando `Status = Ativa`.
- O Worker precisa de um servidor HTTP mínimo — que ele já precisava para
  `/health` e `/metrics`. Não vira uma segunda API: são três rotas.

Na prática é CQRS: a `CampanhasAPI` é o lado de escrita, o `DoacoesWorker` é o
lado de leitura, e o RabbitMQ carrega a projeção de um para o outro.

## Consequências

- O fluxo do vídeo fica mais forte: criar campanha no Swagger da API → mensagem
  na fila → doar → mensagem na fila → consultar o Worker e ver o total. Dois
  serviços cooperando de forma visível.
- Consistência eventual: uma campanha recém-criada aparece no painel segundos
  depois. É o comportamento que o enunciado descreve.
- O Worker ganha um `Service` no Kubernetes (antes não precisaria).

## Alternativa descartada

**API lê o MongoDB do Worker.** Menos código, painel no mesmo Swagger. Mas cria
um segundo canal de acoplamento (o schema do Mongo) além do evento, e cai por
terra o argumento de independência que justifica os dois bancos.
