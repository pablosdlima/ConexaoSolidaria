namespace CampanhasAPI.Domain.Enums;

/// <summary>
/// O que a API sabe sobre a doação. Ela nunca sabe se o Worker processou —
/// só se conseguiu publicar o evento.
/// </summary>
public enum StatusDoacao
{
    Pendente = 1,
    Publicada = 2
}
