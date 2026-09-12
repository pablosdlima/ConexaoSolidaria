namespace DoacoesWorker.Domain.Entities;

/// <summary>
/// Modelo de leitura do painel publico (ADR 003). Um documento por campanha.
/// ValorTotalArrecadado so existe aqui — e so o Worker escreve.
/// </summary>
public class CampanhaResumo
{
    public Guid IdCampanha { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal MetaFinanceira { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset DataInicio { get; set; }
    public DateTimeOffset DataFim { get; set; }
    public decimal ValorTotalArrecadado { get; set; }
    public int QuantidadeDoacoes { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }

    public bool EstaAtiva => Status == "Ativa";
}
