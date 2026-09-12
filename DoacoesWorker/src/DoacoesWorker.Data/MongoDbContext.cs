using DoacoesWorker.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace DoacoesWorker.Data;

/// <summary>
/// Mesmo desenho do MongoDbContext da Fase 3: convencoes camelCase, class maps
/// explicitos e Guid/decimal com serializacao segura (Decimal128 para dinheiro).
/// </summary>
public class MongoDbContext
{
    public const string ColecaoResumos = "campanhas_resumo";
    public const string ColecaoDoacoesProcessadas = "doacoes_processadas";

    private readonly IMongoDatabase _db;

    static MongoDbContext()
    {
        ConventionRegistry.Register("DoacoesWorker", new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        }, _ => true);

        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonSerializer.TryRegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
        BsonSerializer.TryRegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

        BsonClassMap.TryRegisterClassMap<CampanhaResumo>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.IdCampanha);
            cm.UnmapMember(x => x.EstaAtiva);
        });

        BsonClassMap.TryRegisterClassMap<DoacaoProcessada>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.IdDoacao);
        });
    }

    public MongoDbContext(IOptions<MongoOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        _db = client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoCollection<CampanhaResumo> Resumos => _db.GetCollection<CampanhaResumo>(ColecaoResumos);
    public IMongoCollection<DoacaoProcessada> DoacoesProcessadas => _db.GetCollection<DoacaoProcessada>(ColecaoDoacoesProcessadas);

    /// <summary>Indices que o dominio depende. Idempotente; roda no startup.</summary>
    public async Task GarantirIndicesAsync(CancellationToken ct = default)
    {
        await Resumos.Indexes.CreateOneAsync(
            new CreateIndexModel<CampanhaResumo>(Builders<CampanhaResumo>.IndexKeys.Ascending(x => x.Status)),
            cancellationToken: ct);

        await DoacoesProcessadas.Indexes.CreateOneAsync(
            new CreateIndexModel<DoacaoProcessada>(Builders<DoacaoProcessada>.IndexKeys.Ascending(x => x.IdCampanha)),
            cancellationToken: ct);
    }

    public Task PingAsync(CancellationToken ct = default) =>
        _db.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: ct);
}
