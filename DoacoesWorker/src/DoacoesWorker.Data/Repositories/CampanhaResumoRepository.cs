using DoacoesWorker.Domain.Entities;
using DoacoesWorker.Domain.Events;
using DoacoesWorker.Domain.Interfaces.Repositories;
using MongoDB.Driver;

namespace DoacoesWorker.Data.Repositories;

public class CampanhaResumoRepository(MongoDbContext ctx) : ICampanhaResumoRepository
{
    private static readonly UpdateOptions Upsert = new() { IsUpsert = true };

    public async Task<CampanhaResumo?> ObterAsync(Guid idCampanha, CancellationToken ct = default) =>
        await ctx.Resumos.Find(r => r.IdCampanha == idCampanha).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CampanhaResumo>> ListarAtivasAsync(CancellationToken ct = default) =>
        await ctx.Resumos.Find(r => r.Status == "Ativa").SortBy(r => r.DataFim).ToListAsync(ct);

    public Task AtualizarDadosAsync(CampanhaAtualizadaEvent e, CancellationToken ct = default)
    {
        var agora = DateTimeOffset.UtcNow;
        var update = Builders<CampanhaResumo>.Update
            .Set(r => r.Titulo, e.Titulo)
            .Set(r => r.MetaFinanceira, e.MetaFinanceira)
            .Set(r => r.Status, e.Status)
            .Set(r => r.DataInicio, e.DataInicio)
            .Set(r => r.DataFim, e.DataFim)
            .Set(r => r.AtualizadoEm, agora)
            // so na criacao: se a doacao chegou antes, o total ja existe e nao pode ser zerado
            .SetOnInsert(r => r.ValorTotalArrecadado, 0m)
            .SetOnInsert(r => r.QuantidadeDoacoes, 0);

        return ctx.Resumos.UpdateOneAsync(r => r.IdCampanha == e.IdCampanha, update, Upsert, ct);
    }

    public Task IncrementarArrecadadoAsync(Guid idCampanha, decimal valor, CancellationToken ct = default)
    {
        var update = Builders<CampanhaResumo>.Update
            .Inc(r => r.ValorTotalArrecadado, valor)
            .Inc(r => r.QuantidadeDoacoes, 1)
            .Set(r => r.AtualizadoEm, DateTimeOffset.UtcNow);

        return ctx.Resumos.UpdateOneAsync(r => r.IdCampanha == idCampanha, update, Upsert, ct);
    }
}
