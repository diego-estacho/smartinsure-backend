using FluentAssertions;
using SmartInsure.Integration.CalculationEngines.PlugV2;

namespace SmartInsure.Tests.Integration.CalculationEngines;

/// <summary>RN-047/048/049 — ACL do objeto da modalidade PlugV2.</summary>
public class PlugV2ModalityObjectAclMapperTests
{
    [Fact]
    [Trait("RuleId", "RN-047")]
    public void Map_DeveExtrairTagEClausulas_QuandoSucesso()
    {
        const string raw = """
        {"statusCode":200,"hasError":false,"errors":[],
         "response":{"object":{"text":"obj","jsonTag":"{\"campo\":1}"},
         "particularClauses":[{"id":123,"name":"Retencao","text":"t","jsonTag":"{}"}]}}
        """;

        var result = PlugV2ModalityObjectAclMapper.Map(raw);

        result.HasError.Should().BeFalse();
        result.JsonTag.Should().Be("{\"campo\":1}");
        result.ObjectText.Should().Be("obj");
        result.Clauses.Should().ContainSingle();
        result.Clauses[0].ExternalId.Should().Be("123");
        result.Clauses[0].Name.Should().Be("Retencao");
    }

    [Fact]
    [Trait("RuleId", "RN-049")]
    public void Map_DeveSinalizarErro_QuandoHasError()
    {
        const string raw = """{"statusCode":500,"hasError":true,"errors":["x"],"response":null}""";

        var result = PlugV2ModalityObjectAclMapper.Map(raw);

        result.HasError.Should().BeTrue();
        result.Clauses.Should().BeEmpty();
    }

    [Fact]
    [Trait("RuleId", "RN-048")]
    public void Map_DeveIgnorarClausulaSemId()
    {
        const string raw = """
        {"statusCode":200,"hasError":false,"errors":[],
         "response":{"object":{"text":null,"jsonTag":"{}"},
         "particularClauses":[{"name":"Sem id","text":"t","jsonTag":"{}"}]}}
        """;

        var result = PlugV2ModalityObjectAclMapper.Map(raw);

        result.Clauses.Should().BeEmpty();
    }
}
