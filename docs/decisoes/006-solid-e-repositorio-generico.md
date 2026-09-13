# ADR 006 — SOLID e repositório genérico

**Status:** aceita
**Data:** 2026-09-13

## Contexto

Revisão de código após a Fase 7 apontou repetição nos repositórios da API e alguns
pontos onde os princípios SOLID não estavam sendo respeitados. A Fase 3 tinha um
`GenericEntityRepository<T>`, mas com `throw err`, `NotImplementedException` e
`Console.WriteLine` — não foi copiado.

## Decisões

**`IRepository<T>` + `RepositoryBase<T>` (DRY, OCP, LSP).** As operações comuns
(`ObterPorId`, `Adicionar`, `Atualizar`) existem uma vez, em `Data`. Cada
repositório específico herda e acrescenta só as consultas que são dele
(`ListarAsync`, `ObterPorEmailAsync`, `ListarPorDoadorAsync`). A base é `abstract`
com métodos `virtual`: pode ser estendida sem ser modificada, e nenhum filho altera
o contrato. No Worker não há base genérica: os dois repositórios Mongo não têm
operação em comum (`$inc`/upsert vs `TryInsert`) — seria abstração sem reuso.

**`AppException` como base das exceções de negócio (OCP).** Cada exceção declara o
próprio `StatusCode` e `Titulo`. O middleware trata `AppException` uma única vez e
não precisa mudar quando surgir uma exceção nova — antes tinha um `switch` por tipo.

**Regras de transição na entidade (SRP, *tell, don't ask*).** `Campanha.AlterarStatus`
é público e contém a regra: só `Ativa` muda de status, e nunca volta a `Ativa`. O
`CampanhaService` apenas delega. Antes o service tinha um `switch` decidindo qual
método chamar.

**`DbSeeder` separado da migration (SRP).** `DatabaseExtensions` migra; `DbSeeder`
(em `Data`, que já referencia `Domain` e pode usar `ISenhaHasher`) semeia o
GestorONG. Um motivo para mudar em cada classe.

**O que já estava e foi mantido:**
- **ISP** — interfaces pequenas por papel: `ISenhaHasher`, `IJwtGenerator`,
  `IMetricas`, `IEventPublisher`, `IRepository<T>`.
- **DIP** — `Domain` referencia apenas `Microsoft.Extensions.Logging.Abstractions`;
  `Data`, `Messaging` e `IoC` implementam as interfaces dele.
- **OCP no Worker** — `RabbitMQConsumerBase<TEvento, THandler>` é *template method*:
  uma fila nova é uma subclasse de 10 linhas.

**`IGenericService<T>` + `GenericService<T>` (mesma ideia da Fase 3, enxuta).** Os
domain services herdam de uma base que delega o comum ao `IRepository<T>` e
centraliza a regra do 404 (`ObterPorIdAsync` lança `NotFoundException` em um único
lugar). Diferenças em relação à Fase 3, de propósito:
- **sem `Delete`** — nenhum requisito apaga campanha, usuário ou doação; expor a
  operação seria convite a usá-la (ISP);
- **sem `Get()` devolvendo `IQueryable`** — vazaria o EF Core para fora do `Data`
  e permitiria consultas arbitrárias a partir da camada de aplicação;
- **sem `GetByIdInt`, `LastId`, `ListarPaginacao`** — não há uso.

Estrutura resultante (espelha a Fase 3):

```
Domain/Interfaces/Generic/IGenericService.cs     Domain/Services/Generic/GenericService.cs
Domain/Interfaces/Repositories/IRepository.cs    Data/Repositories/RepositoryBase.cs
```

## Alternativa descartada

**Copiar o `GenericServices<T>` da Fase 3 integralmente.** Traria `Delete`,
`IQueryable` e métodos sem uso. Foi adaptado, não copiado.
