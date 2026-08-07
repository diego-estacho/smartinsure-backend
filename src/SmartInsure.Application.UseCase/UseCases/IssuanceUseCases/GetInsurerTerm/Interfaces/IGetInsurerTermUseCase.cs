using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Requests;
using SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Responses;

namespace SmartInsure.Application.UseCase.UseCases.IssuanceUseCases.GetInsurerTerm.Interfaces;

/// <summary>RN-506: lê o Termo vigente da Seguradora da Cotação escolhida, para exibição no emitir.</summary>
public interface IGetInsurerTermUseCase : IUseCase<GetInsurerTermRequest, GetInsurerTermResponse>
{
}
