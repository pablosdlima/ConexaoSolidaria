using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Events;
using CampanhasAPI.Domain.Exceptions;

namespace CampanhasAPI.Domain.Entities;

/// <summary>
/// Intenção de doação registrada pela API. A API guarda o registro e publica o
/// evento; quem soma o valor na campanha é o DoacoesWorker.
/// </summary>
public class Doacao
{
    public Guid Id { get; private set; }
    public Guid IdCampanha { get; private set; }
    public Guid IdDoador { get; private set; }
    public decimal Valor { get; private set; }
    public DateTimeOffset DataHora { get; private set; }
    public StatusDoacao Status { get; private set; }

    private Doacao() { } // EF Core

    public static Doacao Registrar(Campanha campanha, Guid idDoador, decimal valor)
    {
        var agora = DateTimeOffset.UtcNow;

        DomainException.Quando(valor <= 0, "O valor da doação deve ser maior que zero.");
        DomainException.Quando(!campanha.AceitaDoacoes(agora),
            "A campanha não aceita doações: está encerrada ou cancelada.");

        return new Doacao
        {
            Id = Guid.NewGuid(),
            IdCampanha = campanha.Id,
            IdDoador = idDoador,
            Valor = valor,
            DataHora = agora,
            Status = StatusDoacao.Pendente
        };
    }

    public void MarcarPublicada() => Status = StatusDoacao.Publicada;

    public DoacaoRecebidaEvent ParaEvento() => new(Id, IdCampanha, IdDoador, Valor, DataHora);
}
