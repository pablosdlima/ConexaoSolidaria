namespace CampanhasAPI.Data;

/// <summary>Secao "Seed:Gestor" do appsettings.</summary>
public class SeedOptions
{
    public const string Secao = "Seed:Gestor";

    public string Nome { get; set; } = "Gestor da ONG";
    public string? Email { get; set; }
    public string? Senha { get; set; }
    public string Cpf { get; set; } = "52998224725";
}
