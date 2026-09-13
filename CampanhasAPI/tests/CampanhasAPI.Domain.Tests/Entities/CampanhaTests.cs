using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Exceptions;
using static CampanhasAPI.Domain.Tests.Fixtures;

namespace CampanhasAPI.Domain.Tests.Entities;

public class CampanhaTests
{
    [Fact]
    public void Criar_ComDadosValidos_NasceAtivaComOsCamposDoEnunciado()
    {
        var c = Campanha.Criar("  Titulo  ", " Descricao ", Amanha, DaquiUmMes, 5000m);

        Assert.NotEqual(Guid.Empty, c.Id);
        Assert.Equal("Titulo", c.Titulo);          // trim
        Assert.Equal("Descricao", c.Descricao);
        Assert.Equal(5000m, c.MetaFinanceira);
        Assert.Equal(StatusCampanha.Ativa, c.Status);
        Assert.Null(c.DataAtualizacao);
    }

    // RF-2: uma campanha nao pode ser criada com a data de termino no passado
    [Fact]
    public void Criar_ComDataFimNoPassado_Rejeita()
    {
        var ontem = DateTimeOffset.UtcNow.AddDays(-1);
        var ex = Assert.Throws<DomainException>(() =>
            Campanha.Criar("T", "D", ontem.AddDays(-10), ontem, 100m));

        Assert.Contains("passado", ex.Message);
    }

    // RF-2: a meta financeira deve ser maior que zero
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Criar_ComMetaZeroOuNegativa_Rejeita(decimal meta)
    {
        var ex = Assert.Throws<DomainException>(() => Campanha.Criar("T", "D", Amanha, DaquiUmMes, meta));
        Assert.Contains("maior que zero", ex.Message);
    }

    [Fact]
    public void Criar_ComDataFimAntesDaDataInicio_Rejeita() =>
        Assert.Throws<DomainException>(() => Campanha.Criar("T", "D", DaquiUmMes, Amanha, 100m));

    [Theory]
    [InlineData("", "D")]
    [InlineData("   ", "D")]
    [InlineData("T", "")]
    public void Criar_SemTituloOuDescricao_Rejeita(string titulo, string descricao) =>
        Assert.Throws<DomainException>(() => Campanha.Criar(titulo, descricao, Amanha, DaquiUmMes, 100m));

    [Theory]
    [InlineData(StatusCampanha.Concluida)]
    [InlineData(StatusCampanha.Cancelada)]
    public void AlterarStatus_DeAtivaParaFinal_Permite(StatusCampanha novo)
    {
        var c = CampanhaAtiva();
        c.AlterarStatus(novo);

        Assert.Equal(novo, c.Status);
        Assert.NotNull(c.DataAtualizacao);
    }

    [Fact]
    public void AlterarStatus_DeConcluidaParaCancelada_Rejeita()
    {
        var c = CampanhaAtiva();
        c.Concluir();

        Assert.Throws<DomainException>(() => c.Cancelar());
    }

    [Fact]
    public void AlterarStatus_ParaAtiva_Rejeita() =>
        Assert.Throws<DomainException>(() => CampanhaAtiva().AlterarStatus(StatusCampanha.Ativa));

    [Fact]
    public void Atualizar_EmCampanhaConcluida_Rejeita()
    {
        var c = CampanhaAtiva();
        c.Concluir();

        Assert.Throws<DomainException>(() => c.Atualizar("Novo", "Nova", Amanha, DaquiUmMes, 1m));
    }

    [Fact]
    public void AceitaDoacoes_SoQuandoAtivaEDentroDoPrazo()
    {
        var c = CampanhaAtiva();
        Assert.True(c.AceitaDoacoes(DateTimeOffset.UtcNow));
        Assert.False(c.AceitaDoacoes(DaquiUmMes.AddDays(1)));   // depois de DataFim

        c.Cancelar();
        Assert.False(c.AceitaDoacoes(DateTimeOffset.UtcNow));    // cancelada
    }
}
