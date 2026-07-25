using FluentAssertions;
using SmartInsure.Core.Enumerators;
using SmartInsure.Integration.CalculationEngines.PlugV2;

namespace SmartInsure.Tests.Integration.CalculationEngines;

/// <summary>RN-042/RN-046 — ACL de GetAdditionalCoverages (PlugV2 → contrato do motor, ADR-045).</summary>
public class PlugV2AdditionalCoveragesAclMapperTests
{
    private const string ValidResponse = """
    {
      "statusCode": 200,
      "hasError": false,
      "errors": [],
      "response": {
        "additionalCoverages": [
          {
            "branchName": "Garantia",
            "branchCode": "0776",
            "additionalCoverages": {
              "name": "Multa",
              "uniqueId": "11111111-1111-1111-1111-111111111111",
              "insuredAmountCalculationType": 1,
              "allowManualEdit": true
            }
          }
        ]
      }
    }
    """;

    [Fact]
    [Trait("RuleId", "RN-042")]
    public void Map_DeveTraduzirCobertura_QuandoRespostaValida()
    {
        var result = PlugV2AdditionalCoveragesAclMapper.Map(ValidResponse);

        result.IsSuccess.Should().BeTrue();
        result.Coverages.Should().HaveCount(1);
        var coverage = result.Coverages[0];
        coverage.Name.Should().Be("Multa");
        coverage.SourceUniqueId.Should().Be("11111111-1111-1111-1111-111111111111");
        coverage.InsuredAmountCalculationType.Should().Be(1);
        coverage.AllowManualEdit.Should().BeTrue();
        coverage.Branch.Should().Be(ESuretyBranch.Private);
    }

    [Fact]
    [Trait("RuleId", "RN-046")]
    public void Map_DeveMarcarFalha_QuandoHasError()
    {
        const string errorResponse = """
        { "statusCode": 500, "hasError": true, "errors": ["falha na origem"], "response": null }
        """;

        var result = PlugV2AdditionalCoveragesAclMapper.Map(errorResponse);

        result.IsSuccess.Should().BeFalse();
        result.Coverages.Should().BeEmpty();
        result.ErrorMessage.Should().Contain("falha na origem");
    }

    [Fact]
    [Trait("RuleId", "RN-046")]
    public void Map_DeveMarcarFalha_QuandoJsonInvalido()
    {
        var result = PlugV2AdditionalCoveragesAclMapper.Map("não é json");

        result.IsSuccess.Should().BeFalse();
        result.Coverages.Should().BeEmpty();
    }

    [Fact]
    [Trait("RuleId", "RN-042")]
    public void Map_DeveDescartarCobertura_QuandoRamoNaoMapeavel()
    {
        const string unknownBranch = """
        {
          "statusCode": 200, "hasError": false, "errors": [],
          "response": { "additionalCoverages": [
            { "branchName": "Outro", "branchCode": "9999",
              "additionalCoverages": { "name": "X", "uniqueId": "u", "insuredAmountCalculationType": 0, "allowManualEdit": false } }
          ] }
        }
        """;

        var result = PlugV2AdditionalCoveragesAclMapper.Map(unknownBranch);

        result.IsSuccess.Should().BeTrue();
        result.Coverages.Should().BeEmpty();
    }
}
