namespace CampanhasAPI.Application.Dtos.Requests;

public sealed record CadastrarDoadorRequest(string NomeCompleto, string Email, string Cpf, string Senha);
