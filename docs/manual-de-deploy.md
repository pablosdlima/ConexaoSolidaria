# Manual de deploy — Conexão Solidária

Passo a passo testado do zero em 13/09/2026: clone → `kubectl apply` → 8 pods em ~50 s.

## 1. Pré-requisitos

| O quê | Para quê | Como conferir |
|---|---|---|
| **Docker Desktop** com **Kubernetes habilitado** (Settings → Kubernetes → *Enable Kubernetes*) | rodar os containers e o cluster | `docker info` e `kubectl get nodes` → `desktop-control-plane Ready` |
| **kubectl** (vem com o Docker Desktop) | aplicar os manifests | `kubectl version --client` |
| **git** | clonar | `git --version` |
| .NET 10 SDK — **só para desenvolver** (opção B) | compilar localmente | `dotnet --version` |

Memória: reserve ~4 GB para o Docker Desktop (Settings → Resources). A stack completa usa ~1 GB.

Portas que precisam estar livres: **5000, 5001, 5002, 3000, 9090, 5672, 15672, 15692**.
Se você tinha subido a stack pelo `docker compose`, derrube antes: `docker compose -f Orchestration/docker-compose.yml --profile app down`.

## 2. Subir tudo — Opção A (sem compilar; é assim que a correção deve rodar)

As imagens vêm do GitHub Container Registry, publicadas pelo CI a cada push na `main`.

```bash
git clone https://github.com/pablosdlima/ConexaoSolidaria.git
cd ConexaoSolidaria
kubectl apply -k Orchestration/k8s
```

O que você vai ver: `35` linhas terminando em `created` (namespace, secrets, configmaps, PVCs, deployments, services).

Aguarde ~1 minuto e confira:

```bash
kubectl -n conexao-solidaria get pods
```

Esperado — **8 pods `1/1 Running`, `RESTARTS 0`**:

```
api-gateway-…      1/1   Running   0   47s
campanhas-api-…    1/1   Running   0   47s
doacoes-worker-…   1/1   Running   0   47s
grafana-…          1/1   Running   0   47s
mongo-…            1/1   Running   0   47s
postgres-…         1/1   Running   0   47s
prometheus-…       1/1   Running   0   47s
rabbitmq-…         1/1   Running   0   47s
```

Enquanto sobe, é normal ver `Init:0/2` na API e no Worker por alguns segundos: são os
initContainers esperando o Postgres/Mongo e o RabbitMQ responderem. `ContainerCreating` na
primeira vez é o download das imagens (~350 MB cada).

```bash
kubectl -n conexao-solidaria get svc
```

Esperado: os 6 Services `LoadBalancer` com `EXTERNAL-IP` preenchido (não `<pending>`).

## 3. Endereços e credenciais

| O quê | Endereço | Credenciais |
|---|---|---|
| **API Gateway** (entrada única) | http://localhost:5000/api/… | — |
| CampanhasAPI — Swagger | http://localhost:5001/swagger | gestor: `gestor@esperancasolidaria.org` / `Gestor@123` |
| Painel público (Worker) | http://localhost:5002/publico/campanhas | — |
| Grafana | http://localhost:3000 | `admin` / `admin` (ou anônimo) |
| Prometheus | http://localhost:9090/targets | — |
| RabbitMQ UI | http://localhost:15672 | `dev` / `dev` |

O gestor é criado automaticamente no primeiro start (seed). Doadores são cadastrados por `POST /api/doadores`.

## 4. Primeiro fluxo (2 minutos, pelo Swagger em :5001)

1. `POST /api/auth/login` com o gestor → copie o `token` → botão **Authorize** → cole.
2. `POST /api/campanhas` com título, descrição, `dataInicio`, `dataFim` (futura) e `metaFinanceira` > 0 → **201**. Guarde o `id`.
3. `POST /api/doadores` (sem token) com nome, e-mail, CPF válido e senha → **201**.
4. Login com o doador → **Authorize** de novo com o token dele.
5. `POST /api/doacoes` com `idCampanha` e `valorDoacao` → **202 Accepted**.
6. Abra http://localhost:5002/publico/campanhas → a campanha aparece com `valorTotalArrecadado` somado pelo Worker.
7. Abra http://localhost:3000 → dashboard *Conexão Solidária — Observabilidade* com os números.

Todos os passos também funcionam pelo gateway em `http://localhost:5000/api/…`
(o painel fica em `/api/publico/campanhas`).

## 5. Subir tudo — Opção B (compilando localmente; desenvolvimento)

```bash
./Orchestration/deploy-local.sh
```

Faz: build das 3 imagens com a tag do commit → gera o overlay `Orchestration/k8s-local/`
(trocando as imagens do GHCR pelas locais) → `kubectl apply -k` → espera os rollouts.
Leva ~3 min na primeira vez (download do SDK) e ~1 min depois.

Use este caminho sempre que alterar código e quiser ver no cluster **sem esperar o CI**.
Precisa do .NET SDK? Não — o build acontece dentro do Docker. Precisa só do Docker.

## 6. Opção C — só containers, sem Kubernetes (docker compose)

```bash
docker compose -f Orchestration/docker-compose.yml --profile app up -d --build
```

Mesmos endereços da seção 3, exceto Grafana/Prometheus (não estão no compose).
Derrube com `--profile app down` antes de usar o Kubernetes: as portas são as mesmas.

## 7. Atualizar depois de uma mudança no código

| Situação | Comando |
|---|---|
| Fiz push, o CI ficou verde, quero a versão nova no cluster | `kubectl -n conexao-solidaria rollout restart deploy/campanhas-api` (ou `doacoes-worker`, `api-gateway`). Como `imagePullPolicy: Always`, o pod novo baixa o `latest` do GHCR. |
| Quero testar antes do push | `./Orchestration/deploy-local.sh` |

Não use `kubectl rollout restart` depois de um `deploy-local.sh` esperando pegar a imagem
local nova: a tag é do commit; rode o script de novo.

## 8. Derrubar

```bash
kubectl -n conexao-solidaria delete svc --all     # primeiro os Services (ver problema 8.2)
kubectl delete -k Orchestration/k8s
```

Os volumes (PVC) vão junto — dados de teste somem, o gestor é recriado no próximo start.

## 9. Problemas conhecidos e como resolver

Todos aconteceram durante o desenvolvimento e foram resolvidos assim.

### 9.1 Kubernetes não sobe depois de atualizar o Docker Desktop
Sintoma: `kubectl get nodes` → *connection refused*; Settings mostra Kubernetes habilitado mas nunca fica verde.
Causa: containers do cluster antigo (`desktop-control-plane`, `kind-registry`) ficaram órfãos e o novo não consegue substituí-los.
```bash
docker rm -f desktop-control-plane kind-registry desktop-cloud-provider-kind
```
Depois: Settings → Kubernetes → **desmarque** *Enable Kubernetes* → Apply → **marque** → Apply. 2–3 min.

### 9.2 Namespace preso em `Terminating` / Service preso
Sintoma: `kubectl get ns conexao-solidaria` → `Terminating` há minutos. `kubectl describe svc <nome>` mostra
`service.kubernetes.io/load-balancer-cleanup` ou *"container addresses should have 2 values"*.
Causa: um Service `LoadBalancer` cujo container Envoy (`kindccm-…`) ficou quebrado.
```bash
kubectl -n conexao-solidaria describe svc <nome> | grep -o "kindccm-[A-Z0-9]*" | head -1   # pega o nome
docker rm -f kindccm-<NOME>
kubectl -n conexao-solidaria patch svc <nome> -p '{"metadata":{"finalizers":[]}}' --type=merge
```
Se o namespace continuar preso:
```bash
kubectl get ns conexao-solidaria -o json | sed 's/"finalizers": \[[^]]*\]/"finalizers": []/' | kubectl replace --raw /api/v1/namespaces/conexao-solidaria/finalize -f -
```
Prevenção: ao derrubar, apague os Services antes (seção 8).

### 9.3 Um Service `LoadBalancer` fica `<pending>`
Causa mais comum: outra coisa já usa a porta no host (compose de pé, outro processo). No Docker Desktop cada
LoadBalancer vira um container que publica a porta — se a porta está ocupada, ele não nasce.
Confira com `netstat -ano | findstr :5002` e libere. Depois: `kubectl -n conexao-solidaria delete svc <nome>` e `kubectl apply -k Orchestration/k8s`.

### 9.4 Pod da API/Worker reiniciando logo após subir
`kubectl -n conexao-solidaria logs deploy/campanhas-api --previous`. Se for *"Falha na inicializacao — verifique PostgreSQL e RabbitMQ"*,
o banco ou o broker ainda não estavam prontos; os initContainers normalmente evitam isso. Espere 30 s — o Kubernetes tenta de novo sozinho.

### 9.5 Mudei o código, fiz deploy, mas o comportamento é o antigo
O node do Kubernetes guarda cache de imagens por tag. Confira: `kubectl -n conexao-solidaria get pod -l app=campanhas-api -o jsonpath='{.items[0].spec.containers[0].image}'`.
Se aparecer uma tag antiga, rode `./Orchestration/deploy-local.sh` (tag nova a cada commit).

### 9.6 Login devolve 429
Rate limit do gateway: 10 tentativas por minuto por IP. Espere 1 min ou use a API direto em :5001.

### 9.7 `localhost:3000x` (NodePort) não responde
Não usamos NodePort — neste Docker Desktop ele não é encaminhado ao `localhost`. Use as portas dos LoadBalancers da seção 3.
