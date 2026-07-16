namespace SmartInsure.Infra.CrossCutting.Validators;

/// <summary>Validação de CNPJ por dígitos verificadores (RN-007/RN-008).</summary>
public static class CnpjValidator
{
    /// <summary>Normaliza o CNPJ para somente dígitos — forma canônica persistida (RN-007).</summary>
    public static string Normalize(string cnpj)
        => new([.. cnpj.Where(char.IsDigit)]);

    /// <summary>RN-016: matriz é o estabelecimento de ordem /0001 do CNPJ.</summary>
    public static bool IsHeadquarters(string cnpj)
    {
        var digits = Normalize(cnpj);
        return digits.Length == 14 && digits[8..12] == "0001";
    }

    /// <summary>
    /// RN-016: CNPJ da matriz a partir de qualquer estabelecimento da mesma raiz —
    /// raiz (8 dígitos) + ordem 0001 + dígitos verificadores recalculados.
    /// </summary>
    public static string HeadquartersOf(string cnpj)
    {
        var digits = Normalize(cnpj);

        if (digits.Length != 14)
        {
            throw new ArgumentException("O CNPJ deve ter 14 dígitos.", nameof(cnpj));
        }

        var partial = digits[..8] + "0001";
        var first = CheckDigit(partial + "00", [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        var second = CheckDigit(partial + first + "0", [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);

        return partial + first + second;
    }

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
