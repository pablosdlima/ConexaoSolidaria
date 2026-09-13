namespace CampanhasAPI.Domain.Exceptions;

/// <summary>Conflito com o estado atual (e-mail ou CPF ja cadastrado) → HTTP 409.</summary>
public class ConflitoException(string message) : AppException(409, "Conflito", message);
