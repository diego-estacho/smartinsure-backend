using FluentAssertions;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.PlugV2;

namespace SmartInsure.Tests.Integration.CalculationEngines;

/// <summary>RN-033/RN-034 — ACL do PlugV2: traduz GetGroupAndModalities para o contrato do motor.</summary>
[Trait("RuleId", "RN-034")]
public class PlugV2ModalityAclMapperTests
{
    // Amostra fiel ao contrato dev observado (2026-07-22): BranchCode 75=Público, 76=Privado;
    // GlobalModalities[].Id = identificador do motor; ModalityUniqueId = identificador de origem.
    private const string Sample = """
    {
      "StatusCode": 200, "HasError": false, "Errors": [],
      "Response": [
        {
          "Insurance": { "Id": 325, "Name": "Essor Seguros S.A.", "InsuranceUniqueId": "406fecd8-bfd7-4cba-a9bd-87efcfd38288" },
          "IsSuccess": true,
          "GlobalModalities": [
            {
              "Id": 1, "Name": "Adiantamento de Pagamento",
              "Modalities": [
                { "Name": "Adiantamento PF", "BranchCode": "75", "BranchName": "Garantia Segurado - Setor Publico", "ModalityGroupUniqueId": "g-1", "ModalityGroupName": "Financeira", "ModalityGroupType": "GARANTIA_FINANCEIRA", "ModalityUniqueId": "m-1", "MaxPeriodInDays": 720 },
                { "Name": "Adiantamento PJ", "BranchCode": "76", "BranchName": "Garantia Segurado - Setor Privado", "ModalityGroupUniqueId": "g-1", "ModalityGroupName": "Financeira", "ModalityGroupType": "GARANTIA_FINANCEIRA", "ModalityUniqueId": "m-2", "MaxPeriodInDays": 365 }
              ]
            }
          ]
        }
      ]
    }
    """;

    [Fact]
    public void Map_DeveTraduzirSeguradoraEModalidades_DoContratoReal()
    {
        var (result, envelopeError, _) = PlugV2ModalityAclMapper.Map(Sample);

        envelopeError.Should().BeFalse();
        result.Insurers.Should().HaveCount(1);

        var insurer = result.Insurers[0];
        insurer.InsuranceReferenceExternalId.Should().Be("406fecd8-bfd7-4cba-a9bd-87efcfd38288");
        insurer.IsSuccess.Should().BeTrue();
        insurer.Modalities.Should().HaveCount(2);

        var pf = insurer.Modalities.Single(m => m.SourceId == "m-1");
        pf.Branch.Should().Be(ESuretyBranch.Public);
        pf.OriginName.Should().Be("Adiantamento PF");
        pf.EngineModalityId.Should().Be("1");
        pf.EngineModalityName.Should().Be("Adiantamento de Pagamento");
        pf.GroupSourceId.Should().Be("g-1");
        pf.GroupName.Should().Be("Financeira");
        pf.RawParameters.Should().Contain("MaxPeriodInDays");

        insurer.Modalities.Single(m => m.SourceId == "m-2").Branch.Should().Be(ESuretyBranch.Private);
    }

    [Fact]
    public void Map_DeveSinalizarEnvelopeError_QuandoNaoAutenticado()
    {
        const string error = """
        { "StatusCode": 401, "Response": null, "HasError": true, "Errors": ["Usuário não está autenticado!"] }
        """;

        var (_, envelopeError, message) = PlugV2ModalityAclMapper.Map(error);

        envelopeError.Should().BeTrue();
        message.Should().Contain("não está autenticado");
    }

    [Fact]
    public void Map_DeveDescartarModalidade_DeRamoDesconhecido()
    {
        const string weird = """
        {
          "StatusCode": 200, "HasError": false, "Errors": [],
          "Response": [ { "Insurance": { "InsuranceUniqueId": "x", "Name": "X" }, "IsSuccess": true,
            "GlobalModalities": [ { "Id": 9, "Name": "G", "Modalities": [
              { "Name": "Sem ramo", "BranchCode": "99", "ModalityGroupUniqueId": "g", "ModalityGroupName": "G", "ModalityUniqueId": "z" } ] } ] } ]
        }
        """;

        var (result, envelopeError, _) = PlugV2ModalityAclMapper.Map(weird);

        envelopeError.Should().BeFalse();
        result.Insurers[0].Modalities.Should().BeEmpty();
    }
}
