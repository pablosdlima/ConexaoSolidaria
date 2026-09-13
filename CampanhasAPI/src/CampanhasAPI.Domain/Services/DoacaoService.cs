using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Interfaces.Repositories;
using CampanhasAPI.Domain.Interfaces.Services;
using CampanhasAPI.Domain.Services.Generic;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.Domain.Services;

public class DoacaoService(
    IDoacaoRepository doacoes,
    ICampanhaService campanhaService,
    IMetricas metricas,
    ILogger<DoacaoService> logger) : GenericService<Doacao>(doacoes), IDoacaoService
{
    public async Task<Doacao> RegistrarAsync(Guid idCampanha, Guid idDoador, decimal valor, CancellationToken ct = default)
    {
        var campanha = await campanhaService.ObterPorIdAsync(idCampanha, ct);

        // Regras (valor > 0, campanha ativa e no prazo) ficam em Doacao.Registrar
        var doacao = Doacao.Registrar(campanha, idDoador, valor);
        await AdicionarAsync(doacao, ct);

        metricas.DoacaoRegistrada(valor);
        logger.LogInformation("Doacao registrada | Id: {Id} | Campanha: {Campanha} | Valor: {Valor}", doacao.Id, idCampanha, valor);
        return doacao;
    }

    public async Task MarcarPublicadaAsync(Doacao doacao, CancellationToken ct = default)
    {
        doacao.MarcarPublicada();
        await AtualizarAsync(doacao, ct);
    }

    public Task<IReadOnlyList<Doacao>> ListarDoDoadorAsync(Guid idDoador, CancellationToken ct = default) =>
        doacoes.ListarPorDoadorAsync(idDoador, ct);
}
