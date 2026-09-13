namespace CampanhasAPI.Domain.Exceptions;

/// <summary>Recurso nao encontrado → HTTP 404.</summary>
public class NotFoundException(string message) : AppException(404, "Nao encontrado", message);
