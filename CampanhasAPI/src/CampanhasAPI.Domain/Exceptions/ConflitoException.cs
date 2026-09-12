namespace CampanhasAPI.Domain.Exceptions;

/// <summary>Conflito com o estado atual (ex.: e-mail já cadastrado). O middleware converte em HTTP 409.</summary>
public class ConflitoException(string message) : Exception(message);
