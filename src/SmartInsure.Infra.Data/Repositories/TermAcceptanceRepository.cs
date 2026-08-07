using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

/// <summary>Repositório do Aceite do Termo (RN-506): somente-inclusão.</summary>
public sealed class TermAcceptanceRepository(SmartInsureDbContext context)
    : Repository<TermAcceptance>(context), ITermAcceptanceRepository
{
}
