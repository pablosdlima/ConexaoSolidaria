using DoacoesWorker.Domain.Entities;
using DoacoesWorker.Domain.Events;
using DoacoesWorker.Domain.Interfaces.Repositories;
using DoacoesWorker.Domain.Interfaces.Services;
using DoacoesWorker.Domain.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace DoacoesWorker.Domain.Tests.Services;

public class ResumoCampanhaServiceTests
{
    private readonly ICampanhaResumoRepository _resumos = Substitute.For<ICampanhaResumoRepository>();
    private readonly IMetricas _metricas = Substitute.For<IMetricas>();
    private readonly ResumoCampanhaService _sut;

    public ResumoCampanhaServiceTests() =>
        _sut = new ResumoCampanhaService(_resumos, _metricas, NullLogger<ResumoCampanhaService>.Instance);

    [Fact]
    public async Task Atualizar_RepassaOEventoAoRepositorioEContaMetrica()
    {
        var e = new CampanhaAtualizadaEvent(Guid.NewGuid(), "Natal", 1000m, "Ativa", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(1));

        await _sut.AtualizarAsync(e);

        await _resumos.Received(1).AtualizarDadosAsync(e, Arg.Any<CancellationToken>());
        _metricas.Received(1).ResumoAtualizado();
    }

    [Fact]
    public async Task ListarAtivas_DelegaAoRepositorio()
    {
        var esperado = new List<CampanhaResumo> { new() { IdCampanha = Guid.NewGuid(), Titulo = "X", Status = "Ativa" } };
        _resumos.ListarAtivasAsync(Arg.Any<CancellationToken>()).Returns(esperado);

        var resultado = await _sut.ListarAtivasAsync();

        Assert.Same(esperado, resultado);
    }

    [Fact]
    public void CampanhaResumo_EstaAtiva_SoQuandoStatusEAtiva()
    {
        Assert.True(new CampanhaResumo { Status = "Ativa" }.EstaAtiva);
        Assert.False(new CampanhaResumo { Status = "Cancelada" }.EstaAtiva);
        Assert.False(new CampanhaResumo { Status = "Concluida" }.EstaAtiva);
    }
}
