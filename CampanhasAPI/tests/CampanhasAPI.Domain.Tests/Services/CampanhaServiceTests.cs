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

public class CampanhaServiceTests
{
    private readonly ICampanhaRepository _repo = Substitute.For<ICampanhaRepository>();
    private readonly IMetricas _metricas = Substitute.For<IMetricas>();
    private readonly CampanhaService _sut;

    public CampanhaServiceTests() =>
        _sut = new CampanhaService(_repo, _metricas, NullLogger<CampanhaService>.Instance);

    [Fact]
    public async Task Criar_PersisteEContaMetrica()
    {
        var c = await _sut.CriarAsync("T", "D", Amanha, DaquiUmMes, 500m);

        await _repo.Received(1).AdicionarAsync(c, Arg.Any<CancellationToken>());
        _metricas.Received(1).CampanhaCriada();
    }

    [Fact]
    public async Task Criar_ComRegraViolada_NaoPersiste()
    {
        await Assert.ThrowsAsync<DomainException>(() => _sut.CriarAsync("T", "D", Amanha, DaquiUmMes, 0m));

        await _repo.DidNotReceive().AdicionarAsync(Arg.Any<Campanha>(), Arg.Any<CancellationToken>());
        _metricas.DidNotReceive().CampanhaCriada();
    }

    // Regra do 404 centralizada no GenericService
    [Fact]
    public async Task ObterPorId_Inexistente_Lanca404()
    {
        _repo.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Campanha?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.ObterPorIdAsync(Guid.NewGuid()));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task AlterarStatus_ParaConcluida_PersisteAEntidadeAlterada()
    {
        var c = CampanhaAtiva();
        _repo.ObterPorIdAsync(c.Id, Arg.Any<CancellationToken>()).Returns(c);

        var resultado = await _sut.AlterarStatusAsync(c.Id, StatusCampanha.Concluida);

        Assert.Equal(StatusCampanha.Concluida, resultado.Status);
        await _repo.Received(1).AtualizarAsync(c, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AlterarStatus_TransicaoInvalida_NaoPersiste()
    {
        var c = CampanhaAtiva();
        c.Concluir();
        _repo.ObterPorIdAsync(c.Id, Arg.Any<CancellationToken>()).Returns(c);

        await Assert.ThrowsAsync<DomainException>(() => _sut.AlterarStatusAsync(c.Id, StatusCampanha.Cancelada));

        await _repo.DidNotReceive().AtualizarAsync(Arg.Any<Campanha>(), Arg.Any<CancellationToken>());
    }
}
