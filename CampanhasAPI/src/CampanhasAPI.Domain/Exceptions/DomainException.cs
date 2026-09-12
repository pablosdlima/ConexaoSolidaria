namespace CampanhasAPI.Domain.Exceptions;

/// <summary>Violação de regra de negócio. O middleware da API converte em HTTP 400.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public static void Quando(bool condicao, string mensagem)
    {
        if (condicao) throw new DomainException(mensagem);
    }
}
