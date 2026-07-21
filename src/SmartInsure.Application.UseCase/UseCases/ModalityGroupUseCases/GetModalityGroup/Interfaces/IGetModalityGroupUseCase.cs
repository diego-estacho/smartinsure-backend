using SmartInsure.Application.UseCase.Common;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Requests;
using SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Responses;

namespace SmartInsure.Application.UseCase.UseCases.ModalityGroupUseCases.GetModalityGroup.Interfaces;

public interface IGetModalityGroupUseCase : IUseCase<GetModalityGroupRequest, GetModalityGroupResponse>;
