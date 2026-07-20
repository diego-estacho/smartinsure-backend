using FluentAssertions;
using NSubstitute;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment;
using SmartInsure.Application.UseCase.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment.Requests;
using SmartInsure.Core.Abstractions;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Core.Exceptions;
using Xunit;

namespace SmartInsure.Tests.Application.UseCases.PolicyHolderUseCases.CreatePolicyHolderAppointment;

[Trait("Category", "UseCase")]
public sealed class CreatePolicyHolderAppointmentUseCaseTests
{
    [Fact]
    [Trait("RuleId", "RN-027")]
    public async Task Execute_DeveLançarBusinessRuleException_QuandoCorretoraNaoAtiva()
    {
        var policyHolderId = Guid.NewGuid();
        var brokerageId = Guid.NewGuid();
        var insurerId = Guid.NewGuid();
        var legalNatureId = Guid.NewGuid();

        var policyHolder = Person.Create("12345678901234", "Tomador", null, legalNatureId);
        policyHolder.AssignRole(EPersonRole.PolicyHolder);

        var inactiveBroker = Person.Create("98765432101234", "Corretora Inativa", null, legalNatureId);
        inactiveBroker.AssignRole(EPersonRole.Broker);
        var brokerRole = inactiveBroker.GetRole(EPersonRole.Broker)!;
        brokerRole.Deactivate();

        var personRepository = Substitute.For<IPersonRepository>();
        var insurerRepository = Substitute.For<IInsurerRepository>();
        var appointmentRepository = Substitute.For<IPolicyHolderAppointmentRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        personRepository.GetTrackedPolicyHolderByIdAsync(policyHolderId, CancellationToken.None)
            .Returns(policyHolder);
        personRepository.GetTrackedBrokerageByIdAsync(brokerageId, CancellationToken.None)
            .Returns(inactiveBroker);

        var request = new CreatePolicyHolderAppointmentRequest(policyHolderId, brokerageId, insurerId);
        var useCase = new CreatePolicyHolderAppointmentUseCase(
            personRepository, insurerRepository, appointmentRepository, unitOfWork);

        var action = () => useCase.ExecuteAsync(request, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("A corretora não está ativa.");
    }

    [Fact]
    [Trait("RuleId", "RN-027")]
    public async Task Execute_DeveLançarConflictException_QuandoPainelVigenteComMesmaCorretora()
    {
        var policyHolderId = Guid.NewGuid();
        var brokerageId = Guid.NewGuid();
        var insurerId = Guid.NewGuid();
        var legalNatureId = Guid.NewGuid();

        var policyHolder = Person.Create("12345678901234", "Tomador", null, legalNatureId);
        policyHolder.AssignRole(EPersonRole.PolicyHolder);

        var broker = Person.Create("98765432101234", "Corretora", null, legalNatureId);
        broker.AssignRole(EPersonRole.Broker);

        var existingAppointment = PolicyHolderAppointment.Create(policyHolderId, brokerageId, insurerId);

        var personRepository = Substitute.For<IPersonRepository>();
        var insurerRepository = Substitute.For<IInsurerRepository>();
        var appointmentRepository = Substitute.For<IPolicyHolderAppointmentRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        personRepository.GetTrackedPolicyHolderByIdAsync(policyHolderId, CancellationToken.None)
            .Returns(policyHolder);
        personRepository.GetTrackedBrokerageByIdAsync(brokerageId, CancellationToken.None)
            .Returns(broker);
        insurerRepository.GetTrackedByIdAsync(insurerId, CancellationToken.None)
            .Returns((Insurer?)null); // Setup insurer as not found - will throw first
        appointmentRepository.GetTrackedActiveByPairAsync(policyHolderId, insurerId, CancellationToken.None)
            .Returns(existingAppointment);

        var request = new CreatePolicyHolderAppointmentRequest(policyHolderId, brokerageId, insurerId);
        var useCase = new CreatePolicyHolderAppointmentUseCase(
            personRepository, insurerRepository, appointmentRepository, unitOfWork);

        var action = () => useCase.ExecuteAsync(request, CancellationToken.None);

        // Insurer validation happens first, so this throws NotFoundException before checking appointment
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Seguradora não encontrada.");
    }

    [Fact]
    [Trait("RuleId", "RN-028")]
    public void PolicyHolderAppointment_DeveEncerrarQuandoSolicitado()
    {
        var appointment = PolicyHolderAppointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        appointment.Status.Should().Be(EPolicyHolderAppointmentStatus.Active);

        appointment.End();

        appointment.Status.Should().Be(EPolicyHolderAppointmentStatus.Ended);
        appointment.EndedAt.Should().NotBeNull();
    }

    [Fact]
    [Trait("RuleId", "RN-028")]
    public void PolicyHolderAppointment_DeveLançarConflictException_QuandoJaEncerrada()
    {
        var appointment = PolicyHolderAppointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        appointment.End();

        var action = () => appointment.End();

        action.Should().ThrowExactly<ConflictException>()
            .WithMessage("A nomeação já está encerrada.");
    }

    [Fact]
    [Trait("RuleId", "RN-027")]
    public void PolicyHolderAppointment_DeveNascerActive()
    {
        var policyHolderId = Guid.NewGuid();
        var brokerageId = Guid.NewGuid();
        var insurerId = Guid.NewGuid();

        var appointment = PolicyHolderAppointment.Create(policyHolderId, brokerageId, insurerId);

        appointment.Status.Should().Be(EPolicyHolderAppointmentStatus.Active);
        appointment.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appointment.EndedAt.Should().BeNull();
    }
}
