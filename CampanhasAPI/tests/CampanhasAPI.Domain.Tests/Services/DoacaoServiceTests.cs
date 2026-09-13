using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Exceptions;
using CampanhasAPI.Domain.Interfaces.Repositories;
using CampanhasAPI.Domain.Interfaces.Services;
using CampanhasAPI.Domain.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using static CampanhasAPI.Domain.Tests.Fixtures;

namespace CampanhasAPI.Domain.Tests.Services;

public class DoacaoServiceTests
{
    private readonly IDoacaoRepository _repo = Substitute.For<IDoacaoRepository>();
    private readonly ICampanhaService _campanhas = Substitute.For<ICampanhaService>();
    private readonly IMetricas _metricas = Substitute.For<IMetricas>();
    private readonly DoacaoService _sut;

    public DoacaoServiceTests() =>
        _sut = new DoacaoService(_repo, _campanhas, _metricas, NullLogger<DoacaoService>.Instance);

    [Fact]
    public async Task Registrar_EmCampanhaAtiva_PersistePendenteEContaMetrica()
    {
        var campanha = CampanhaAtiva();
        _campanhas.ObterPorIdAsync(campanha.Id, Arg.Any<CancellationToken>()).Returns(campanha);

        var d = await _sut.RegistrarAsync(campanha.Id, Guid.NewGuid(), 120m);

        Assert.Equal(StatusDoacao.Pendente, d.Status);
        await _repo.Received(1).AdicionarAsync(d, Arg.Any<CancellationToken>());
        _metricas.Received(1).DoacaoRegistrada(120m);
    }

    [Fact]
    public async Task Registrar_EmCampanhaInexistente_Lanca404ENaoPersiste()
    {
        _campanhas.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Campanha>(_ => throw new NotFoundException("nao existe"));

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RegistrarAsync(Guid.NewGuid(), Guid.NewGuid(), 10m));

        await _repo.DidNotReceive().AdicionarAsync(Arg.Any<Doacao>(), Arg.Any<CancellationToken>());
        _metricas.DidNotReceive().DoacaoRegistrada(Arg.Any<decimal>());
    }

    // RF-5: campanha cancelada nao aceita doacao — e nada e gravado
    [Fact]
    public async Task Registrar_EmCampanhaCancelada_Lanca400ENaoPersiste()
    {
        var campanha = CampanhaAtiva();
        campanha.Cancelar();
        _campanhas.ObterPorIdAsync(campanha.Id, Arg.Any<CancellationToken>()).Returns(campanha);

        var ex = await Assert.ThrowsAsync<DomainException>(() => _sut.RegistrarAsync(campanha.Id, Guid.NewGuid(), 10m));

        Assert.Equal(400, ex.StatusCode);
        await _repo.DidNotReceive().AdicionarAsync(Arg.Any<Doacao>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarcarPublicada_AtualizaStatusEPersiste()
    {
        var d = Doacao.Registrar(CampanhaAtiva(), Guid.NewGuid(), 1m);

        await _sut.MarcarPublicadaAsync(d);

        Assert.Equal(StatusDoacao.Publicada, d.Status);
        await _repo.Received(1).AtualizarAsync(d, Arg.Any<CancellationToken>());
    }
}
