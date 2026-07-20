using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.EndPolicyHolderAppointment;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.EndPolicyHolderAppointment.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.PolicyHolderUseCases.EndPolicyHolderAppointment;

[Trait("Category", "UseCase")]
public sealed class EndPolicyHolderAppointmentUseCaseTests
{
    private readonly IPolicyHolderAppointmentRepository _appointmentRepository = Substitute.For<IPolicyHolderAppointmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    [Trait("RuleId", "RN-028")]
    public async Task Execute_DeveEncerrarNomeação_QuandoVigente()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = PolicyHolderAppointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _appointmentRepository.GetTrackedByIdAsync(appointmentId, CancellationToken.None)
            .Returns(appointment);

        var request = new EndPolicyHolderAppointmentRequest(appointmentId);
        var useCase = new EndPolicyHolderAppointmentUseCase(_appointmentRepository, _unitOfWork);

        await useCase.ExecuteAsync(request, CancellationToken.None);

        appointment.Status.Should().Be(EPolicyHolderAppointmentStatus.Ended);
        appointment.EndedAt.Should().NotBeNull();
        await _unitOfWork.Received(1).CommitAsync(CancellationToken.None);
    }

    [Fact]
    [Trait("RuleId", "RN-028")]
    public async Task Execute_DeveLançarConflictException_QuandoJaEncerrada()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = PolicyHolderAppointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        appointment.End(); // Já encerrada

        _appointmentRepository.GetTrackedByIdAsync(appointmentId, CancellationToken.None)
            .Returns(appointment);

        var request = new EndPolicyHolderAppointmentRequest(appointmentId);
        var useCase = new EndPolicyHolderAppointmentUseCase(_appointmentRepository, _unitOfWork);

        var action = () => useCase.ExecuteAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("A nomeação já está encerrada.");
    }

    [Fact]
    [Trait("RuleId", "RN-028")]
    public async Task Execute_DeveLançarNotFoundException_QuandoIdInexistente()
    {
        _appointmentRepository.GetTrackedByIdAsync(Guid.NewGuid(), CancellationToken.None)
            .Returns((PolicyHolderAppointment?)null);

        var request = new EndPolicyHolderAppointmentRequest(Guid.NewGuid());
        var useCase = new EndPolicyHolderAppointmentUseCase(_appointmentRepository, _unitOfWork);

        var action = () => useCase.ExecuteAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Nomeação não encontrada.");
    }

    [Fact]
    [Trait("RuleId", "RN-028")]
    public async Task Execute_DeveEncerrarSemValidarStatusCorretoraSegurador()
    {
        // RN-028: Encerramento não é afetado pelo status da Corretora ou da Seguradora
        var appointmentId = Guid.NewGuid();
        var appointment = PolicyHolderAppointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _appointmentRepository.GetTrackedByIdAsync(appointmentId, CancellationToken.None)
            .Returns(appointment);

        var request = new EndPolicyHolderAppointmentRequest(appointmentId);
        var useCase = new EndPolicyHolderAppointmentUseCase(_appointmentRepository, _unitOfWork);

        // Deve encerrar sem checar se broker/insurer estão ativos
        await useCase.ExecuteAsync(request, CancellationToken.None);

        appointment.Status.Should().Be(EPolicyHolderAppointmentStatus.Ended);
    }
}
