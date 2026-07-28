namespace SmartInsure.Core.Abstractions.Repositories.Dtos;

/// <summary>RN-052: Filial do Tomador — Pessoa jurídica vinculada à matriz.</summary>
public sealed record PersonBranchDto(Guid Id, string DocumentNumber, string Name, string? SocialName);
