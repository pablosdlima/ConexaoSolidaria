# ADR 002 — Estrutura do repositório

**Status:** aceita
**Data:** 2026-09-11

## Contexto

O enunciado pede um repositório público com README único para subir tudo. A
arquitetura tem dois microsserviços. O padrão já praticado na Fase 3 do curso é
um repositório por serviço, cada um com `src/` em camadas, `dockerfile` e `k8s/`.

## Decisão

**Monorepo com serviços autocontidos.** Cada pasta de serviço é idêntica a um
repositório da Fase 3 — poderia ser extraída para um repositório próprio sem
nenhuma alteração interna:

```
CampanhasAPI/                 DoacoesWorker/
├── CampanhasAPI.slnx         ├── DoacoesWorker.slnx
├── dockerfile                ├── dockerfile
├── k8s/                      ├── k8s/
└── src/                      └── src/
    ├── CampanhasAPI.Api          ├── DoacoesWorker.Worker
    ├── CampanhasAPI.Application  ├── DoacoesWorker.Application
    ├── CampanhasAPI.Domain       ├── DoacoesWorker.Domain
    ├── CampanhasAPI.Data         ├── DoacoesWorker.Data
    ├── CampanhasAPI.IoC          ├── DoacoesWorker.IoC
    └── CampanhasAPI.Messaging    └── DoacoesWorker.Messaging
```

A raiz guarda só o que é transversal — e **nenhuma solution**: `docker-compose`
(infra + serviços), `docs/` e o pipeline. Cada serviço tem a própria solution;
o CI compila e gera a imagem de cada um em jobs separados.

### Dependências entre camadas (iguais à Fase 3)

```
Api/Worker ──► IoC ──► Application ──► Domain
     │          │  └──► Messaging ──► Application, Domain
     └──────────┴──► Data ──► Domain
```

`Domain` não referencia ninguém. `Data` e `Messaging` são detalhes de
infraestrutura que implementam interfaces do `Domain`.

### Contrato de evento duplicado, não compartilhado

`DoacaoRecebidaEvent` existe em `CampanhasAPI.Domain/Events` **e** em
`DoacoesWorker.Domain/Events`. Os serviços se acoplam ao **formato da mensagem**
(JSON), nunca a um assembly comum. É o mesmo padrão da Fase 3 e da aplicação de
referência eShop da Microsoft. O custo é manter dois arquivos de cinco linhas
iguais; o ganho é que nenhum serviço precisa do outro para compilar — o
`dockerfile` de cada um usa a própria pasta como contexto de build.

## Alternativas descartadas

- **Um repositório por serviço.** Fiel à Fase 3, mas o README de "como subir
  tudo" e o pipeline ficariam espalhados; os professores teriam dois links
  para clonar e sincronizar. A estrutura escolhida permite essa divisão depois,
  se necessário.
- **Projeto `Contracts` compartilhado.** Elimina a duplicação, mas cria uma
  dependência de compilação entre os serviços e obriga o contexto Docker a ser
  a raiz do repositório.
- **Projeto `Authentication` separado**, como na Fase 3. Lá ele estava vazio —
  a configuração de JWT vivia em `IoC`. Não foi replicado.
