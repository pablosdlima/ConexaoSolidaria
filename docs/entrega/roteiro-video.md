# Roteiro do vídeo — máximo 15 minutos

O enunciado diz: *"não deve focar em ler código linha a linha, mas em comprovar a
arquitetura e o funcionamento"*. Cada bloco abaixo prova um item obrigatório (3.a–3.d).
Tempos são alvos; o total tem 1 min de folga.

## Antes de gravar (checklist de 5 minutos)

- [ ] Docker Desktop aberto, Kubernetes verde
- [ ] `./Orchestration/deploy-local.sh` executado; `kubectl -n conexao-solidaria get pods` → tudo `Running`, `0` restarts
- [ ] Abas abertas, nesta ordem: diagrama · GitHub Actions · terminal · Grafana (:3000) · RabbitMQ UI (:15672, dev/dev) · Swagger API (:5001/swagger) · painel (:5002/publico/campanhas)
- [ ] Grafana com o dashboard **Conexão Solidária — Observabilidade** aberto e `refresh 5s`
- [ ] Postman/Swagger com o body da campanha e da doação já preenchidos (copiar do fim deste arquivo)
- [ ] Zoom do navegador em 125% — o texto precisa ser legível no vídeo

## Bloco 1 — Diagrama de arquitetura (3.a) · 0:00 → 2:30

Tela: `docs/arquitetura.md` no GitHub (ou o PNG exportado do draw.io).

Falar, apontando:
1. **Dois microsserviços**, cada um com seu banco: CampanhasAPI + PostgreSQL, DoacoesWorker + MongoDB. *"O único contrato entre eles é o evento."*
2. **RabbitMQ** no meio: a API publica `DoacaoRecebidaEvent`, o Worker consome. *"A API não soma nada — ela nem tem o campo ValorTotalArrecadado."*
3. **Painel público no Worker**: quem tem o dado, serve o dado (CQRS).
4. **API Gateway** na borda: entrada única, rate limit no login.
5. **Prometheus + Grafana** coletando dos pods por descoberta automática.
6. Mencionar: *"cada decisão dessas tem um ADR no repositório, em docs/decisoes."*

## Bloco 2 — Pipeline de CI (3.b) · 2:30 → 4:30

Tela: GitHub → Actions → última execução.

1. Mostrar o gatilho: *"a cada push na main"*. Abrir a execução: **3 jobs em paralelo**, um por serviço.
2. Expandir o job `campanhas-api`: `dotnet build` → **`Testes de unidade`** (61 verdes) → **`Build da imagem`** → push no GHCR.
3. Ir em *Packages* do repositório: as 3 imagens, com a tag = **SHA do commit**. *"Tag imutável: o que passou no pipeline é exatamente o que roda."*
4. Opcional se sobrar tempo: clicar *Run workflow* e deixar rodando ao fundo.

## Bloco 3 — Kubernetes e Grafana (3.c) · 4:30 → 7:00

Tela: terminal.

```bash
kubectl -n conexao-solidaria get pods
kubectl -n conexao-solidaria get svc
```

Falar: 8 pods — 3 serviços, RabbitMQ, Postgres, Mongo, Prometheus, Grafana. Zero restarts.
*"Os initContainers esperam banco e broker antes de subir a API."*

Tela: Grafana. Mostrar os 4 stats do topo e o gráfico **"Fluxo assíncrono: registradas × processadas"**.
*"Vamos voltar aqui depois da doação para ver os números mudarem."*

## Bloco 4 — Funcionamento (3.d) · 7:00 → 13:00

### 4.i Autenticação e JWT · 7:00 → 8:00
Swagger da API → `POST /api/auth/login` com `gestor@esperancasolidaria.org` / `Gestor@123`.
Copiar o token → botão **Authorize**. *"A role GestorONG vem dentro do token."*
Mostrar rápido: `GET /api/campanhas` sem token → 401.

### 4.ii Criação de campanha · 8:00 → 9:00
`POST /api/campanhas` com o payload do fim deste arquivo → 201.
Ir ao painel público (:5002) → a campanha aparece com **0**.
*"O Worker já recebeu o evento CampanhaAtualizada e criou o resumo no Mongo."*

Opcional (20 s): tentar `metaFinanceira: 0` → 400 com a mensagem da regra.

### 4.iii Doação, fila e painel · 9:00 → 12:00
1. **Authorize** com um doador (`joao@exemplo.com` / `Senha@123` — cadastrar antes via `POST /api/doadores` se o banco estiver limpo).
2. `POST /api/doacoes` com o id da campanha e `valorDoacao: 150` → **202 Accepted**. *"202, não 201: o processamento é assíncrono."*
3. **RabbitMQ UI** → Queues → `doacoes-recebidas`: mostrar o gráfico de mensagens (publish / deliver / ack). Mostrar também as filas `.dlq` vazias.
4. **Painel público** → recarregar → `valorTotalArrecadado: 150`, `quantidadeDoacoes: 1`. *"Quem somou foi o Worker."*
5. `kubectl -n conexao-solidaria logs deploy/doacoes-worker --tail=5` → linha *"Doacao … somada | +150"*.
6. **Grafana** → stat "Doações processadas" subiu; o gráfico registradas × processadas mostra as duas linhas juntas.

### 4.iv Prova extra que vale muito (se couber) · 12:00 → 13:00
Idempotência: na RabbitMQ UI, na exchange `conexao-solidaria`, **publicar manualmente** a mesma mensagem
(mesmo `idDoacao`, routing key `doacao.recebida`). Painel continua 150. Grafana: "Reentregas ignoradas" = 1.
Log do Worker: *"ja processada — reentrega ignorada"*.
*"O ledger no Mongo garante exactly-once mesmo com o broker reentregando."*

## Bloco 5 — Encerramento · 13:00 → 14:00

Tela: README no GitHub.
- *"Um comando sobe tudo: `kubectl apply -k Orchestration/k8s`."*
- Bônus entregues: 68 testes de unidade rodando no CI; API Gateway com Ocelot.
- Documentação: 7 ADRs + manual de deploy.

## Payloads prontos para copiar

```json
{ "email": "gestor@esperancasolidaria.org", "senha": "Gestor@123" }
```
```json
{ "titulo": "Natal Solidário 2026", "descricao": "Presentes para 200 crianças acolhidas",
  "dataInicio": "2026-11-01T00:00:00Z", "dataFim": "2026-12-24T23:59:59Z", "metaFinanceira": 25000 }
```
```json
{ "nomeCompleto": "João Pereira", "email": "joao@exemplo.com", "cpf": "111.444.777-35", "senha": "Senha@123" }
```
```json
{ "email": "joao@exemplo.com", "senha": "Senha@123" }
```
```json
{ "idCampanha": "COLE-O-ID-AQUI", "valorDoacao": 150 }
```
