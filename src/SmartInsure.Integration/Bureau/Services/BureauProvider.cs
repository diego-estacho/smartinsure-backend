using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Refit;
using SmartInsure.Core.Abstractions.Services;
using SmartInsure.Core.Abstractions.Services.Dtos;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.Bureau.Interfaces;
using SmartInsure.Integration.Bureau.Models;

namespace SmartInsure.Integration.Bureau.Services;

/// <summary>
/// Implementação do Birô sobre o gateway InsurePoint.
/// RN-003: cada chamada consulta a fonte; resposta com status diferente de OK é
/// consulta sem dado. RN-004: falha de comunicação vira null logado, nunca exceção.
/// </summary>
public sealed class BureauProvider(
    IBureauApi api,
    ILogger<BureauProvider> logger) : IBureauProvider
{
    public async Task<BureauPersonComplement?> GetPersonComplementAsync(
        string cpfCnpj,
        string personType,
        EBureau bureau,
        CancellationToken cancellationToken)
    {
        var request = new BureauPersonComplementRequest
        {
            CpfCnpj = cpfCnpj,
            TipoPessoa = personType,
            BureauChoices = [bureau.ToString()],
        };

        BureauPersonComplementResponse response;

        try
        {
            response = await api.GetPersonComplementAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Cancelamento pedido pelo chamador não é falha do birô — propaga.
            throw;
        }
        catch (Exception exception) when (exception
            is ApiException
            or HttpRequestException
            or TimeoutException
            or OperationCanceledException
            or TimeoutRejectedException
            or BrokenCircuitException)
        {
            // RN-004: indisponibilidade (incluindo tempo de resposta excedido) não bloqueia o fluxo.
            logger.LogError(exception,
                "Falha na consulta ao birô {Bureau} para o documento {CpfCnpj}", bureau, cpfCnpj);

            return null;
        }

        if (!response.IsOk)
        {
            // RN-003 (caso limite): insucesso na fonte é consulta sem dado.
            logger.LogWarning(
                "Bureau {Bureau} respondeu sem dado para o documento {CpfCnpj}: status {Status}, mensagem {Message}",
                bureau, cpfCnpj, response.Status, response.Message);

            return null;
        }

        return Map(response);
    }

    internal static BureauPersonComplement Map(BureauPersonComplementResponse response) => new()
    {
        Document = response.Cnpj,
        Name = response.Nome,
        TradeName = response.Fantasia,
        RegistrationStatus = response.Situacao,
        RegistrationStatusDate = response.DataSituacao,
        OpeningDate = response.Abertura,
        LegalNature = response.NaturezaJuridica,
        CompanySize = response.Porte,
        ShareCapital = response.CapitalSocial,
        Street = response.Logradouro,
        Number = response.Numero,
        AddressComplement = response.Complemento,
        District = response.Bairro,
        City = response.Municipio,
        State = response.Uf,
        ZipCode = response.Cep,
        Phone = response.Telefone,
        Email = response.Email,
        MainActivities = [.. response.AtividadePrincipal.Select(a => new BureauEconomicActivity(a.Code, a.Text))],
        SecondaryActivities = [.. response.AtividadesSecundarias.Select(a => new BureauEconomicActivity(a.Code, a.Text))],
    };
}
