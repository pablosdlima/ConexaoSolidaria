using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Exceptions;
using static CampanhasAPI.Domain.Tests.Fixtures;

namespace CampanhasAPI.Domain.Tests.Entities;

public class DoacaoTests
{
    [Fact]
    public void Registrar_EmCampanhaAtiva_NascePendente()
    {
        var campanha = CampanhaAtiva();
        var idDoador = Guid.NewGuid();

        var d = Doacao.Registrar(campanha, idDoador, 150m);

        Assert.Equal(campanha.Id, d.IdCampanha);
        Assert.Equal(idDoador, d.IdDoador);
        Assert.Equal(150m, d.Valor);
        Assert.Equal(StatusDoacao.Pendente, d.Status);
    }

    // RF-5: a doacao nao pode ser feita para campanhas encerradas ou canceladas
    [Fact]
    public void Registrar_EmCampanhaCancelada_Rejeita()
    {
        var campanha = CampanhaAtiva();
        campanha.Cancelar();

        var ex = Assert.Throws<DomainException>(() => Doacao.Registrar(campanha, Guid.NewGuid(), 10m));
        Assert.Contains("encerrada ou cancelada", ex.Message);
    }

    [Fact]
    public void Registrar_EmCampanhaConcluida_Rejeita()
    {
        var campanha = CampanhaAtiva();
        campanha.Concluir();

        Assert.Throws<DomainException>(() => Doacao.Registrar(campanha, Guid.NewGuid(), 10m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Registrar_ComValorZeroOuNegativo_Rejeita(decimal valor) =>
        Assert.Throws<DomainException>(() => Doacao.Registrar(CampanhaAtiva(), Guid.NewGuid(), valor));

    [Fact]
    public void ParaEvento_CarregaOsCamposDoContrato()
    {
        var d = Doacao.Registrar(CampanhaAtiva(), Guid.NewGuid(), 99.9m);

        var e = d.ParaEvento();

        Assert.Equal(d.Id, e.IdDoacao);           // chave de idempotencia do Worker
        Assert.Equal(d.IdCampanha, e.IdCampanha);
        Assert.Equal(d.IdDoador, e.IdDoador);
        Assert.Equal(d.Valor, e.Valor);
        Assert.Equal(d.DataHora, e.DataHora);
    }

    [Fact]
    public void MarcarPublicada_MudaOStatus()
    {
        var d = Doacao.Registrar(CampanhaAtiva(), Guid.NewGuid(), 1m);
        d.MarcarPublicada();
        Assert.Equal(StatusDoacao.Publicada, d.Status);
    }
}
