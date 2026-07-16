namespace SmartInsure.Infra.CrossCutting.Validators;

/// <summary>Validação de CNPJ por dígitos verificadores (RN-005/RN-006).</summary>
public static class CnpjValidator
{
    /// <summary>Normaliza o CNPJ para somente dígitos — forma canônica persistida (RN-005).</summary>
    public static string Normalize(string cnpj)
        => new([.. cnpj.Where(char.IsDigit)]);

    public static bool IsValid(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
        {
            return false;
        }

        var digits = Normalize(cnpj);

        if (digits.Length != 14 || digits.Distinct().Count() == 1)
        {
            return false;
        }

        return digits[12] - '0' == CheckDigit(digits, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2])
            && digits[13] - '0' == CheckDigit(digits, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
    }

    private static int CheckDigit(string digits, int[] weights)
    {
        var sum = weights.Select((weight, index) => weight * (digits[index] - '0')).Sum();
        var mod = sum % 11;

        return mod < 2 ? 0 : 11 - mod;
    }
}
