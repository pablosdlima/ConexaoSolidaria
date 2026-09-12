namespace CampanhasAPI.Domain.Exceptions;

/// <summary>Recurso não encontrado. O middleware da API converte em HTTP 404.</summary>
public class NotFoundException(string message) : Exception(message);
