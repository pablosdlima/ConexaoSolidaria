namespace DoacoesWorker.Data;

/// <summary>Secao "MongoDB" do appsettings. Em Kubernetes vem de ConfigMap + Secret.</summary>
public class MongoOptions
{
    public const string Secao = "MongoDB";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "doacoes";
}
