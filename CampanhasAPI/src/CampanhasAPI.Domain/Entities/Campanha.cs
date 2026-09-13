using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Exceptions;

namespace CampanhasAPI.Domain.Entities;

/// <summary>
/// Campanha de arrecadação. Repare que NÃO existe ValorTotalArrecadado aqui:
/// esse número pertence ao DoacoesWorker e vive no banco dele (ADR 001 e 003).
/// </summary>
public class Campanha
{
    public Guid Id { get; private set; }
    public string Titulo { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public DateTimeOffset DataInicio { get; private set; }
    public DateTimeOffset DataFim { get; private set; }
    public decimal MetaFinanceira { get; private set; }
    public StatusCampanha Status { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }
    public DateTimeOffset? DataAtualizacao { get; private set; }

    private Campanha() { } // EF Core

    public static Campanha Criar(string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira)
    {
        Validar(titulo, descricao, dataInicio, dataFim, metaFinanceira);

        return new Campanha
        {
            Id = Guid.NewGuid(),
            Titulo = titulo.Trim(),
            Descricao = descricao.Trim(),
            DataInicio = dataInicio.ToUniversalTime(),
            DataFim = dataFim.ToUniversalTime(),
            MetaFinanceira = metaFinanceira,
            Status = StatusCampanha.Ativa,
            DataCriacao = DateTimeOffset.UtcNow
        };
    }

    public void Atualizar(string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira)
    {
        DomainException.Quando(Status != StatusCampanha.Ativa, "Somente campanhas ativas podem ser editadas.");
        Validar(titulo, descricao, dataInicio, dataFim, metaFinanceira);

        Titulo = titulo.Trim();
        Descricao = descricao.Trim();
        DataInicio = dataInicio.ToUniversalTime();
        DataFim = dataFim.ToUniversalTime();
        MetaFinanceira = metaFinanceira;
        DataAtualizacao = DateTimeOffset.UtcNow;
    }

    public void Concluir() => AlterarStatus(StatusCampanha.Concluida);
    public void Cancelar() => AlterarStatus(StatusCampanha.Cancelada);

    /// <summary>RF-5: doação só em campanha ativa e dentro do prazo.</summary>
    public bool AceitaDoacoes(DateTimeOffset agora) =>
        Status == StatusCampanha.Ativa && agora <= DataFim;

    /// <summary>Unica transicao permitida: Ativa → Concluida ou Cancelada. A regra mora aqui, nao no service.</summary>
    public void AlterarStatus(StatusCampanha novo)
    {
        DomainException.Quando(novo == StatusCampanha.Ativa, "Uma campanha nao pode voltar a ser Ativa.");
        DomainException.Quando(Status != StatusCampanha.Ativa,
            $"Campanha {Status} não pode mudar para {novo}. Só campanhas ativas mudam de status.");
        Status = novo;
        DataAtualizacao = DateTimeOffset.UtcNow;
    }

    private static void Validar(string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira)
    {
        DomainException.Quando(string.IsNullOrWhiteSpace(titulo), "O título é obrigatório.");
        DomainException.Quando(string.IsNullOrWhiteSpace(descricao), "A descrição é obrigatória.");
        DomainException.Quando(metaFinanceira <= 0, "A meta financeira deve ser maior que zero.");
        DomainException.Quando(dataFim < DateTimeOffset.UtcNow, "A data de término não pode estar no passado.");
        DomainException.Quando(dataFim < dataInicio, "A data de término não pode ser anterior à data de início.");
    }
}
