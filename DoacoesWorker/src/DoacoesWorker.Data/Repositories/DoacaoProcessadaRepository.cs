using DoacoesWorker.Domain.Entities;
using DoacoesWorker.Domain.Interfaces.Repositories;
using MongoDB.Driver;

namespace DoacoesWorker.Data.Repositories;

public class DoacaoProcessadaRepository(MongoDbContext ctx) : IDoacaoProcessadaRepository
{
    public async Task<bool> TentarRegistrarAsync(DoacaoProcessada doacao, CancellationToken ct = default)
    {
        try
        {
            await ctx.DoacoesProcessadas.InsertOneAsync(doacao, cancellationToken: ct);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false; // _id (IdDoacao) ja existe: reentrega
        }
    }

    public Task RemoverAsync(Guid idDoacao, CancellationToken ct = default) =>
        ctx.DoacoesProcessadas.DeleteOneAsync(d => d.IdDoacao == idDoacao, ct);
}
