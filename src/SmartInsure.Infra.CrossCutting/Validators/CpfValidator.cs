namespace SmartInsure.Infra.CrossCutting.Validators;

/// <summary>Validação de CPF por dígitos verificadores (RN-082). Espelha o <see cref="CnpjValidator"/>.</summary>
public static class CpfValidator
{
    /// <summary>Normaliza o CPF para somente dígitos — forma canônica persistida (RN-082).</summary>
    public static string Normalize(string cpf)
        => new([.. cpf.Where(char.IsDigit)]);

    public static bool IsValid(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return false;
        }

        var digits = Normalize(cpf);

        if (digits.Length != 11 || digits.Distinct().Count() == 1)
        {
            return false;
        }

        return digits[9] - '0' == CheckDigit(digits, [10, 9, 8, 7, 6, 5, 4, 3, 2])
            && digits[10] - '0' == CheckDigit(digits, [11, 10, 9, 8, 7, 6, 5, 4, 3, 2]);
    }

    private static int CheckDigit(string digits, int[] weights)
    {
        var sum = weights.Select((weight, index) => weight * (digits[index] - '0')).Sum();
        var mod = sum % 11;

        return mod < 2 ? 0 : 11 - mod;
    }
}
