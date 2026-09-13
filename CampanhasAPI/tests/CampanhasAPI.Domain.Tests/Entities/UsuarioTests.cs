using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Exceptions;
using static CampanhasAPI.Domain.Tests.Fixtures;

namespace CampanhasAPI.Domain.Tests.Entities;

public class UsuarioTests
{
    [Fact]
    public void Criar_NormalizaEmailECpf()
    {
        var u = Usuario.Criar("  Maria  ", "  Maria@Exemplo.COM ", "111.444.777-35", "hash", Role.Doador);

        Assert.Equal("Maria", u.NomeCompleto);
        Assert.Equal("maria@exemplo.com", u.Email);   // minusculo: o indice unico depende disso
        Assert.Equal("11144477735", u.Cpf);           // so digitos
        Assert.Equal(Role.Doador, u.Role);
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("")]
    [InlineData("a@")]
    public void Criar_ComEmailInvalido_Rejeita(string email) =>
        Assert.Throws<DomainException>(() => Usuario.Criar("M", email, CpfValido, "hash", Role.Doador));

    [Fact]
    public void Criar_ComCpfInvalido_Rejeita() =>
        Assert.Throws<DomainException>(() => Usuario.Criar("M", "m@x.com", "123.456.789-00", "hash", Role.Doador));

    [Fact]
    public void Criar_SemHashDeSenha_Rejeita() =>
        Assert.Throws<DomainException>(() => Usuario.Criar("M", "m@x.com", CpfValido, "", Role.Doador));

    [Fact]
    public void Role_ToString_EhOTextoExatoDoEnunciado()
    {
        // A claim do JWT e o RequireRole usam esses textos literalmente
        Assert.Equal("GestorONG", Role.GestorONG.ToString());
        Assert.Equal("Doador", Role.Doador.ToString());
    }
}
