using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Application.Dtos.Responses;

public sealed record UsuarioResponse(Guid Id, string NomeCompleto, string Email, string Role)
{
    public static UsuarioResponse De(Usuario u) => new(u.Id, u.NomeCompleto, u.Email, u.Role.ToString());
}
