namespace CampanhasAPI.Domain.Events;

/// <summary>
/// Publicado pela CampanhasAPI quando um doador registra uma intenção de doação.
/// Consumido pelo DoacoesWorker, que é o único responsável por somar o valor
/// em ValorTotalArrecadado da campanha.
/// </summary>
/// <remarks>
/// Cada serviço mantém sua própria cópia deste contrato (mesmo padrão da Fase 3):
/// os serviços se acoplam apenas ao formato da mensagem, nunca a um assembly.
/// IdDoacao é a chave de idempotência — o Worker deve ignorar um id já processado,
/// porque o broker pode reentregar.
/// </remarks>
public sealed record DoacaoRecebidaEvent(
    Guid IdDoacao,
    Guid IdCampanha,
    Guid IdDoador,
    decimal Valor,
    DateTimeOffset DataHora);
