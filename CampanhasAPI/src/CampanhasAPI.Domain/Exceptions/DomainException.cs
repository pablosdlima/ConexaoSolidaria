namespace CampanhasAPI.Domain.Exceptions;

/// <summary>Violacao de regra de negocio → HTTP 400.</summary>
public class DomainException(string message) : AppException(400, "Regra de negocio violada", message)
{
    public static void Quando(bool condicao, string mensagem)
    {
        if (condicao) throw new DomainException(mensagem);
    }
}
