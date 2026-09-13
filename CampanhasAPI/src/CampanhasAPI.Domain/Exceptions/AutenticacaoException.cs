namespace CampanhasAPI.Domain.Exceptions;

/// <summary>Credenciais invalidas → HTTP 401.</summary>
public class AutenticacaoException(string message) : AppException(401, "Nao autenticado", message);
