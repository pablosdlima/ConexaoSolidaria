using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Application.Interfaces;

public interface IJwtGenerator
{
    (string Token, DateTimeOffset ExpiraEm) Gerar(Usuario usuario);
}
