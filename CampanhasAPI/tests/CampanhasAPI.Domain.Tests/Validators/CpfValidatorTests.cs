using CampanhasAPI.Domain.Validators;

namespace CampanhasAPI.Domain.Tests.Validators;

public class CpfValidatorTests
{
    [Theory]
    [InlineData("111.444.777-35")]
    [InlineData("11144477735")]
    [InlineData("529.982.247-25")]
    [InlineData(" 529 982 247 25 ")]
    public void EhValido_ComCpfCorreto_RetornaTrue(string cpf) =>
        Assert.True(CpfValidator.EhValido(cpf));

    [Theory]
    [InlineData("111.444.777-36")]   // ultimo digito verificador errado
    [InlineData("111.444.777-45")]   // primeiro digito verificador errado
    [InlineData("123.456.789-00")]
    public void EhValido_ComDigitoVerificadorErrado_RetornaFalse(string cpf) =>
        Assert.False(CpfValidator.EhValido(cpf));

    [Theory]
    [InlineData("000.000.000-00")]
    [InlineData("111.111.111-11")]   // passa no calculo, mas e invalido por convencao
    [InlineData("999.999.999-99")]
    public void EhValido_ComTodosOsDigitosIguais_RetornaFalse(string cpf) =>
        Assert.False(CpfValidator.EhValido(cpf));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("1234567890")]       // 10 digitos
    [InlineData("123456789012")]     // 12 digitos
    [InlineData("abc.def.ghi-jk")]
    public void EhValido_ComFormatoInvalido_RetornaFalse(string? cpf) =>
        Assert.False(CpfValidator.EhValido(cpf));

    [Fact]
    public void Normalizar_RemoveTudoQueNaoEDigito() =>
        Assert.Equal("11144477735", CpfValidator.Normalizar("111.444.777-35"));
}
