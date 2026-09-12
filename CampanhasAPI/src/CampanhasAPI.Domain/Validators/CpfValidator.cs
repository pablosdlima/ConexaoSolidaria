namespace CampanhasAPI.Domain.Validators;

public static class CpfValidator
{
    private static readonly int[] Multiplicadores1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] Multiplicadores2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

    /// <summary>Remove pontos, traço e espaços: "123.456.789-09" vira "12345678909".</summary>
    public static string Normalizar(string cpf) =>
        new(cpf.Where(char.IsDigit).ToArray());

    /// <summary>Valida formato (11 dígitos), rejeita sequências repetidas e confere os dois dígitos verificadores.</summary>
    public static bool EhValido(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;

        var digitos = Normalizar(cpf);
        if (digitos.Length != 11) return false;
        if (digitos.Distinct().Count() == 1) return false; // 111.111.111-11 passa no cálculo, mas é inválido

        var base9 = digitos[..9];
        var dv1 = CalcularDigito(base9, Multiplicadores1);
        var dv2 = CalcularDigito(base9 + dv1, Multiplicadores2);

        return digitos[9] - '0' == dv1 && digitos[10] - '0' == dv2;
    }

    private static int CalcularDigito(string parte, int[] multiplicadores)
    {
        var soma = 0;
        for (var i = 0; i < multiplicadores.Length; i++)
            soma += (parte[i] - '0') * multiplicadores[i];

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
