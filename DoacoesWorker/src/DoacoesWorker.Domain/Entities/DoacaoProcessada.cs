namespace DoacoesWorker.Domain.Entities;

/// <summary>
/// Ledger de idempotencia: um registro por IdDoacao ja somada. Indice unico
/// garante que a mesma doacao nunca e contada duas vezes, mesmo se o broker
/// reentregar a mensagem.
/// </summary>
public class DoacaoProcessada
{
    public Guid IdDoacao { get; set; }
    public Guid IdCampanha { get; set; }
    public Guid IdDoador { get; set; }
    public decimal Valor { get; set; }
    public DateTimeOffset DataHoraDoacao { get; set; }
    public DateTimeOffset ProcessadaEm { get; set; }
}
