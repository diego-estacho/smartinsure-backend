using SmartInsure.Core.Entities;
using SmartInsure.Core.Exceptions;

namespace SmartInsure.Application.UseCase.Services.QuotationGroups;

/// <summary>
/// RN-503 — replica para a oferta o endereço do Segurado escolhido pelo corretor. A escolha vem do
/// cadastro da Pessoa (fonte única); a oferta guarda a cópia. Sem escolha, vale o endereço principal.
/// Usado na criação e na atualização do Grupo: reconfirmar o endereço re-replica os valores atuais.
///
/// Segurado sem endereço — ou com endereço incompleto para emitir — **não** impede montar a oferta: a
/// oferta fica sem réplica e o bloqueio acontece no emitir, com o motivo (RN-503, casos limite; o portão
/// da RN-500 verifica). Já um endereço informado que não é do Segurado é dado inválido, e é recusado aqui.
/// </summary>
internal static class InsuredAddressReplicator
{
    internal static void Replicate(QuotationGroup group, Person insured, Guid? insuredAddressId)
    {
        // Sem escolha informada e oferta já com réplica: preserva o que foi combinado. Atualização que
        // não passou pela etapa do Segurado (reidratar a oferta e salvar de novo) não troca o endereço
        // por conta própria — trocar é ação explícita do corretor, que reenvia o id escolhido.
        if (insuredAddressId is null && group.InsuredAddress is not null)
        {
            return;
        }

        var address = insuredAddressId is null
            ? insured.Addresses.FirstOrDefault(candidate => candidate.IsMain)
            : insured.Addresses.FirstOrDefault(candidate => candidate.Id == insuredAddressId.Value);

        if (address is null)
        {
            if (insuredAddressId is not null)
            {
                throw new BusinessRuleException("O endereço informado não pertence ao segurado desta oferta.");
            }

            // Segurado ainda sem endereço: segue montando a oferta; o emitir cobra (RN-500/RN-503).
            return;
        }

        try
        {
            group.ReplicateInsuredAddress(
                address.ZipCode,
                address.Street,
                address.Number,
                address.Complement,
                address.Neighborhood,
                address.City,
                address.State);
        }
        catch (BusinessRuleException)
        {
            // Endereço cadastrado mas incompleto para emitir: a oferta não trava aqui — quem cobra é o
            // portão do emitir, que explica o que falta e manda corrigir no cadastro do Segurado.
        }
    }
}
