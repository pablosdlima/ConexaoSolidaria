namespace CampanhasAPI.Domain.Interfaces.Services;

public interface ISenhaHasher
{
    string Hash(string senha);
    bool Verificar(string senha, string hash);
}
