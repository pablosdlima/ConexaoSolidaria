#!/usr/bin/env sh
# Build das duas imagens com a tag do commit atual e deploy no Kubernetes local.
#
#   ./Orchestration/deploy-local.sh          # build + apply + set image
#
# Por que a tag do commit e nao :local? O node do Kubernetes tem cache proprio de
# imagens: com a mesma tag, um "rollout restart" sobe a imagem ANTIGA. Tag imutavel
# por commit e o que o CI (Fase 8) tambem faz.
set -e
cd "$(dirname "$0")"

TAG="$(git rev-parse --short HEAD)"
git diff --quiet || TAG="$TAG-dirty"
export IMAGE_TAG="$TAG"

echo "==> build das imagens :$TAG"
docker compose --profile app build

echo "==> aplicando manifests"
kubectl apply -k k8s

echo "==> atualizando deployments para :$TAG"
kubectl -n conexao-solidaria set image deploy/campanhas-api  campanhas-api=conexao-solidaria/campanhas-api:$TAG
kubectl -n conexao-solidaria set image deploy/doacoes-worker doacoes-worker=conexao-solidaria/doacoes-worker:$TAG
kubectl -n conexao-solidaria set image deploy/api-gateway   api-gateway=conexao-solidaria/api-gateway:$TAG
kubectl -n conexao-solidaria rollout status deploy/campanhas-api  --timeout=180s
kubectl -n conexao-solidaria rollout status deploy/doacoes-worker --timeout=180s
kubectl -n conexao-solidaria rollout status deploy/api-gateway   --timeout=180s

echo
kubectl -n conexao-solidaria get pods
echo
echo "API Gateway:   http://localhost:5000/api/..."
echo "CampanhasAPI:  http://localhost:5001/swagger"
echo "Painel:        http://localhost:5002/publico/campanhas"
echo "RabbitMQ UI:   http://localhost:15672  (dev / dev)"
