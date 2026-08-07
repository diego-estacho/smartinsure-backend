using SmartInsure.Core.Entities;

namespace SmartInsure.Core.Abstractions.Repositories;

/// <summary>
/// Repositório do Aceite do Termo (RN-506): registro somente-inclusão — aceite não se edita nem se
/// apaga, é prova do que foi aceito e quando.
/// </summary>
public interface ITermAcceptanceRepository : IRepository<TermAcceptance>
{
}
