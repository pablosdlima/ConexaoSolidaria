# ADR 005 — Observabilidade

**Status:** aceita
**Data:** 2026-09-13

## Contexto

O enunciado exige `/health` ou `/metrics` e um dashboard no Grafana com **métricas
reais** da aplicação (CPU/memória dos pods ou requisições HTTP). A Fase 3 já usava
`prometheus-net` com targets estáticos no Prometheus.

## Decisões

**Prometheus + Grafana no cluster, em `Orchestration/k8s/monitoring/`.** Zabbix foi
descartado: o enunciado aceita qualquer um dos dois, e Prometheus é o padrão do
ecossistema Kubernetes — descoberta de pods, cAdvisor e o plugin do RabbitMQ falam
Prometheus nativamente.

**Descoberta de pods por anotação, não targets estáticos.** Os Deployments levam
`prometheus.io/scrape|port|path`; o Prometheus descobre os pods via API do
Kubernetes (`kubernetes_sd_configs: role: pod`). Escalar o Worker para 3 réplicas
não exige editar configuração — os três aparecem sozinhos.

**Três fontes de métrica, três perguntas diferentes:**
| Fonte | Responde | Exemplo |
|---|---|---|
| `/metrics` dos serviços (prometheus-net) | a aplicação está saudável? | `http_requests_received_total`, latência p95 |
| Métricas de negócio (`IMetricas`) | o fluxo assíncrono está fechando? | `doacoes_processadas_total`, `doacoes_duplicadas_ignoradas_total` |
| RabbitMQ (`rabbitmq_prometheus`, `/metrics/per-object`) | a fila está acumulando? | `rabbitmq_queue_messages{queue="doacoes-recebidas"}` |
| cAdvisor via kubelet | os pods cabem nos limites? | `container_memory_working_set_bytes` |

**Métricas de negócio atrás de uma interface (`IMetricas`).** O `Domain` não referencia
`prometheus-net`; a implementação (`PrometheusMetricas`) vive no `IoC`. Os contadores
são campos de instância de um Singleton — como `static`, o C# só os criaria no
primeiro uso e o dashboard mostraria "No data" em vez de `0` (testado).

**Health checks reais.** `/health` executa `CanConnect` no Postgres/ping no Mongo e
confere a conexão com o RabbitMQ, devolvendo JSON com cada dependência. As probes
do Kubernetes usam o mesmo endpoint: um pod cujo banco caiu sai do Service sozinho.

**Serilog com saída no console.** Em Kubernetes o console é o canal de log
(`kubectl logs`). Request logging estruturado, com probes e scrapes rebaixados para
`Debug` para não poluir. O sink de MongoDB da Fase 3 não foi replicado: adicionaria
escrita em banco a cada requisição sem o enunciado pedir.

**Dashboard provisionado por ConfigMap (kustomize `configMapGenerator`).** O JSON
fica em `monitoring/grafana/dashboards/` e sobe junto com o cluster — nada de
importar à mão no vídeo. Login anônimo como Viewer para abrir direto.

## Alternativas descartadas

- **OpenTelemetry + Collector.** Mais completo (traces), mas mais peças no cluster
  local. `prometheus-net` já estava no repertório do time.
- **kube-prometheus-stack (Helm).** Instala tudo, mas esconde a configuração — o
  entregável são os `.yaml`, e queremos poder explicá-los.
