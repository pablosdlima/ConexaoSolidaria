using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;

namespace CampanhasAPI.Domain.Tests;

/// <summary>Objetos validos de partida — cada teste altera so o que quer provar.</summary>
public static class Fixtures
{
    public static readonly DateTimeOffset Amanha = DateTimeOffset.UtcNow.AddDays(1);
    public static readonly DateTimeOffset DaquiUmMes = DateTimeOffset.UtcNow.AddMonths(1);

    public const string CpfValido = "111.444.777-35";
    public const string CpfValido2 = "529.982.247-25";

    public static Campanha CampanhaAtiva(decimal meta = 1000m) =>
        Campanha.Criar("Natal Solidario", "Presentes para as criancas", Amanha, DaquiUmMes, meta);

    public static Usuario Doador() =>
        Usuario.Criar("Maria da Silva", "maria@exemplo.com", CpfValido, "hash-bcrypt", Role.Doador);
}
