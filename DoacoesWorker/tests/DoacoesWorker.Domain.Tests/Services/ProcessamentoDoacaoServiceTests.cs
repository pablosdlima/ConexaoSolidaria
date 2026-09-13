using DoacoesWorker.Domain.Entities;
using DoacoesWorker.Domain.Events;
using DoacoesWorker.Domain.Interfaces.Repositories;
using DoacoesWorker.Domain.Interfaces.Services;
using DoacoesWorker.Domain.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DoacoesWorker.Domain.Tests.Services;

/// <summary>
/// O unico lugar do sistema que escreve ValorTotalArrecadado. Estes testes provam
/// a garantia central do RT-Mensageria: uma doacao e somada exatamente uma vez.
/// </summary>
public class ProcessamentoDoacaoServiceTests
{
    private readonly IDoacaoProcessadaRepository _ledger = Substitute.For<IDoacaoProcessadaRepository>();
    private readonly ICampanhaResumoRepository _resumos = Substitute.For<ICampanhaResumoRepository>();
    private readonly IMetricas _metricas = Substitute.For<IMetricas>();
    private readonly ProcessamentoDoacaoService _sut;

    private static readonly DoacaoRecebidaEvent Evento = new(
        IdDoacao: Guid.NewGuid(), IdCampanha: Guid.NewGuid(), IdDoador: Guid.NewGuid(),
        Valor: 150m, DataHora: DateTimeOffset.UtcNow);

    public ProcessamentoDoacaoServiceTests()
    {
        _metricas.MedirProcessamento().Returns(Substitute.For<IDisposable>());
        _sut = new ProcessamentoDoacaoService(_ledger, _resumos, _metricas, NullLogger<ProcessamentoDoacaoService>.Instance);
    }

    [Fact]
    public async Task Processar_PrimeiraVez_RegistraNoLedgerESomaNaCampanha()
    {
        _ledger.TentarRegistrarAsync(Arg.Any<DoacaoProcessada>(), Arg.Any<CancellationToken>()).Returns(true);

        var processou = await _sut.ProcessarAsync(Evento);

        Assert.True(processou);
        await _ledger.Received(1).TentarRegistrarAsync(
            Arg.Is<DoacaoProcessada>(d => d.IdDoacao == Evento.IdDoacao && d.Valor == 150m), Arg.Any<CancellationToken>());
        await _resumos.Received(1).IncrementarArrecadadoAsync(Evento.IdCampanha, 150m, Arg.Any<CancellationToken>());
        _metricas.Received(1).DoacaoProcessada(150m);
    }

    // Idempotencia: o broker reentregou a mesma mensagem
    [Fact]
    public async Task Processar_Reentrega_IgnoraENaoSomaDeNovo()
    {
        _ledger.TentarRegistrarAsync(Arg.Any<DoacaoProcessada>(), Arg.Any<CancellationToken>()).Returns(false);

        var processou = await _sut.ProcessarAsync(Evento);

        Assert.False(processou);
        await _resumos.DidNotReceive().IncrementarArrecadadoAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>());
        _metricas.Received(1).DoacaoDuplicadaIgnorada();
        _metricas.DidNotReceive().DoacaoProcessada(Arg.Any<decimal>());
    }

    // Compensacao: se o $inc falhar, desfaz o ledger para a reentrega poder tentar de novo
    [Fact]
    public async Task Processar_QuandoIncrementoFalha_RemoveDoLedgerEPropaga()
    {
        _ledger.TentarRegistrarAsync(Arg.Any<DoacaoProcessada>(), Arg.Any<CancellationToken>()).Returns(true);
        _resumos.IncrementarArrecadadoAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("mongo fora do ar"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ProcessarAsync(Evento));

        await _ledger.Received(1).RemoverAsync(Evento.IdDoacao, Arg.Any<CancellationToken>());
        _metricas.DidNotReceive().DoacaoProcessada(Arg.Any<decimal>());
    }

    [Fact]
    public async Task Processar_LedgerAntesDoIncremento_OrdemImporta()
    {
        // A ordem e o que garante exactly-once sob concorrencia: quem vence o insert unico e quem soma.
        _ledger.TentarRegistrarAsync(Arg.Any<DoacaoProcessada>(), Arg.Any<CancellationToken>()).Returns(true);

        await _sut.ProcessarAsync(Evento);

        Received.InOrder(async () =>
        {
            await _ledger.TentarRegistrarAsync(Arg.Any<DoacaoProcessada>(), Arg.Any<CancellationToken>());
            await _resumos.IncrementarArrecadadoAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>());
        });
    }
}
