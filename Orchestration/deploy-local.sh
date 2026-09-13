#!/usr/bin/env sh
# Build das tres imagens com a tag do commit atual e deploy no Kubernetes local.
#
#   ./Orchestration/deploy-local.sh
#
# Os manifests em k8s/ apontam para as imagens publicadas no GHCR (caminho de quem so quer
# rodar: kubectl apply -k Orchestration/k8s). Para desenvolver sem esperar o CI, este script
# gera um overlay (k8s-local/, ignorado pelo git) que troca as imagens pelas locais, com a
# tag do commit — tag imutavel, porque o node do Kubernetes tem cache proprio (ADR 004).
set -e
cd "$(dirname "$0")"

TAG="$(git rev-parse --short HEAD)"
git diff --quiet || TAG="$TAG-dirty"
export IMAGE_TAG="$TAG"

echo "==> build das imagens :$TAG"
docker compose --profile app build

echo "==> gerando overlay k8s-local com as imagens locais"
mkdir -p k8s-local
cat > k8s-local/kustomization.yaml <<K
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
resources:
  - ../k8s
images:
  - name: ghcr.io/pablosdlima/conexaosolidaria/campanhas-api
    newName: conexao-solidaria/campanhas-api
    newTag: "$TAG"
  - name: ghcr.io/pablosdlima/conexaosolidaria/doacoes-worker
    newName: conexao-solidaria/doacoes-worker
    newTag: "$TAG"
  - name: ghcr.io/pablosdlima/conexaosolidaria/api-gateway
    newName: conexao-solidaria/api-gateway
    newTag: "$TAG"
patches:
  - target: { kind: Deployment, name: campanhas-api }
    patch: |
      - { op: replace, path: /spec/template/spec/containers/0/imagePullPolicy, value: IfNotPresent }
  - target: { kind: Deployment, name: doacoes-worker }
    patch: |
      - { op: replace, path: /spec/template/spec/containers/0/imagePullPolicy, value: IfNotPresent }
  - target: { kind: Deployment, name: api-gateway }
    patch: |
      - { op: replace, path: /spec/template/spec/containers/0/imagePullPolicy, value: IfNotPresent }
K

echo "==> kubectl apply -k k8s-local"
kubectl apply -k k8s-local

for d in campanhas-api doacoes-worker api-gateway; do
  kubectl -n conexao-solidaria rollout status deploy/$d --timeout=180s
done

echo
kubectl -n conexao-solidaria get pods
echo
echo "API Gateway:   http://localhost:5000/api/..."
echo "CampanhasAPI:  http://localhost:5001/swagger"
echo "Painel:        http://localhost:5002/publico/campanhas"
echo "Grafana:       http://localhost:3000"
echo "RabbitMQ UI:   http://localhost:15672  (dev / dev)"
