namespace CampanhasAPI.Domain.Exceptions;

/// <summary>Credenciais inválidas. O middleware converte em HTTP 401.</summary>
public class AutenticacaoException(string message) : Exception(message);
