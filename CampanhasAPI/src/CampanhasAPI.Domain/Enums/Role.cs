namespace CampanhasAPI.Domain.Enums;

/// <summary>
/// Perfis de acesso. Os nomes viram a claim "role" do JWT e são usados
/// literalmente em [Authorize(Roles = "GestorONG")] — não renomear.
/// </summary>
public enum Role
{
    GestorONG = 1,
    Doador = 2
}
