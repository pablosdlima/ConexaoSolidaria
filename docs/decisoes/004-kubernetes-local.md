# ADR 004 — Kubernetes local e exposição dos serviços

**Status:** aceita
**Data:** 2026-09-12

## Contexto

O enunciado exige a aplicação num cluster Kubernetes (Minikube, Kind ou Docker
Desktop) com os `.yaml` entregues. O time já tem Docker Desktop instalado.

## Decisões

**Cluster: o Kubernetes embutido no Docker Desktop.** Zero instalação extra, e as
imagens buildadas pelo `docker compose` ficam visíveis para o cluster sem registry.
Versões recentes provisionam o cluster com *kind* por baixo (node
`desktop-control-plane`), o que traz duas consequências abaixo.

**Namespace `conexao-solidaria`.** Isola os recursos e permite derrubar tudo com um
comando.

**`type: LoadBalancer`, não `NodePort`.** No Kubernetes baseado em kind do Docker
Desktop, NodePort **não** é encaminhado para `localhost` (testado: conexão recusada).
LoadBalancer ganha IP externo e é exposto em `localhost:<port>`. Portas: API 5001,
painel do Worker 5002, RabbitMQ 5672/15672.

**Tag de imagem imutável (SHA do commit).** O node tem cache próprio de imagens:
com a tag fixa `:local`, um `rollout restart` sobe a imagem *antiga* (testado —
digests diferentes entre o Docker e o pod). `Orchestration/deploy-local.sh` builda
com a tag do commit e faz `kubectl set image`. O CI da Fase 8 segue a mesma regra.

**`initContainers` esperando banco e broker.** A API aplica migrations e declara a
topologia do RabbitMQ no startup. Sem a espera, o primeiro start falha e o pod
reinicia — o `kubectl get pods` do vídeo mostraria `RESTARTS > 0`. Com `busybox`
+ `nc -z`, os 5 pods sobem com zero restarts.

**Um `kustomization.yaml` por serviço + um agregador em `Orchestration/k8s`.** Cada
serviço é dono dos próprios manifests (ConfigMap, Secret, Deployment, Service); o
agregador junta infra e serviços: `kubectl apply -k Orchestration/k8s`.

**Secrets em `stringData`.** Legível no repositório; o Kubernetes converte para
base64. São credenciais de desenvolvimento — em produção viriam de um cofre.

**PersistentVolumeClaim para RabbitMQ, Postgres e Mongo.** `strategy: Recreate` nos
bancos, porque um volume `ReadWriteOnce` não pode ser montado por dois pods
durante um rolling update.

## Alternativas descartadas

- **Minikube / Kind instalados à parte.** Funcionariam, mas exigem `minikube image
  load` / `kind load docker-image` a cada build — mais um passo para o README.
- **Ingress.** Um único ponto de entrada seria mais elegante, mas exige instalar um
  ingress controller. Fica como evolução junto com o API Gateway (bônus).

## Adendos (2026-09-13, após o gateway e o clone limpo)

**Manifests apontam para o GHCR; o overlay local troca pelas imagens da máquina.**
`kubectl apply -k Orchestration/k8s` sobe a solução em qualquer máquina só com o Docker,
puxando as imagens que o CI publicou. `deploy-local.sh` gera `Orchestration/k8s-local/`
(ignorado pelo git) com um `images:` do kustomize substituindo as três pelas locais com a
tag do commit, e aplica esse overlay. Antes o script fazia `kubectl set image` depois do
`apply` — e qualquer `apply` seguinte revertia para o GHCR silenciosamente (aconteceu).

**Uma porta por Service LoadBalancer.** No Docker Desktop, cada LoadBalancer vira um
container Envoy (`kindccm-…`) que publica as portas do Service no host. Dois Services
expondo a mesma porta (a API e o Worker tinham uma porta "interna" 8080 para o gateway)
fazem o segundo falhar em `<pending>` — e deixam um container quebrado que o cloud provider
não consegue limpar, prendendo o Service em `Terminating` com o finalizer
`service.kubernetes.io/load-balancer-cleanup`. O gateway agora usa as portas dos próprios
Services (`campanhas-api:5001`, `doacoes-worker:5002`); o compose faz os containers
escutarem nessas mesmas portas para o `ocelot.json` ser único.

**Namespace preso em `Terminating`.** Sintoma do caso acima. Solução documentada no manual
de deploy: remover o container `kindccm-…` pelo nome (aparece nos eventos do Service) e
limpar o finalizer do Service com `kubectl patch`.
