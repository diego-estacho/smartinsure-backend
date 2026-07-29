using Microsoft.EntityFrameworkCore;
using SmartInsure.Core.Abstractions.Repositories;
using SmartInsure.Core.Abstractions.Repositories.Dtos;
using SmartInsure.Core.Entities;
using SmartInsure.Core.Enumerators;
using SmartInsure.Infra.Data.Context;

namespace SmartInsure.Infra.Data.Repositories;

public sealed class PersonRepository(SmartInsureDbContext context)
    : Repository<Person>(context), IPersonRepository
{
    public async Task<IReadOnlyList<PersonSearchItemDto>> SearchByNameOrDocumentAsync(
        string nameTerm,
        string? documentNumber,
        bool headquartersOnly,
        CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking()
            .Where(person => person.Name.Contains(nameTerm)
                || (person.SocialName != null && person.SocialName.Contains(nameTerm))
                || (documentNumber != null && person.DocumentNumber == documentNumber));

        if (headquartersOnly)
        {
            // RN-016: tomador é sempre a matriz (pessoa jurídica de ordem /0001).
            query = query.Where(person => person.Type == EPersonType.J
                && person.DocumentNumber.Substring(8, 4) == "0001");
        }

        // Ordena pela entidade ANTES de projetar — o EF não traduz OrderBy por propriedade de um
        // DTO construído (PersonSearchItemDto), o que causava InvalidOperationException na busca.
        return await ProjectItems(query.OrderBy(person => person.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<PersonSearchItemDto?> GetByDocumentNumberAsync(
        string documentNumber, CancellationToken cancellationToken)
        => await ProjectItems(Set.AsNoTracking().Where(person => person.DocumentNumber == documentNumber))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Person?> GetTrackedByDocumentNumberAsync(
        string documentNumber, CancellationToken cancellationToken)
        => await Set
            .Include(person => person.Roles)
            .FirstOrDefaultAsync(
                person => person.DocumentNumber == documentNumber, cancellationToken);

    public async Task<Person?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .Include(person => person.Roles)
            .FirstOrDefaultAsync(person => person.Id == id, cancellationToken);

    public async Task<PersonSearchItemDto?> GetSummaryByIdAsync(Guid id, CancellationToken cancellationToken)
        => await ProjectItems(Set.AsNoTracking().Where(person => person.Id == id))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<BrokerageListResult> ListBrokeragesAsync(
        BrokerageListQuery query,
        CancellationToken cancellationToken)
    {
        var enablements = Context.Set<BrokerageInsurerEnablement>().AsNoTracking();

        // Base: Pessoas jurídicas com Papel da Pessoa de corretor (RN-018).
        var baseQuery = Set.AsNoTracking()
            .Where(person => person.Type == EPersonType.J
                && person.Roles.Any(role => role.Role == EPersonRole.Broker));

        // Busca livre: CNPJ (dígitos), razão social e nome fantasia.
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            baseQuery = baseQuery.Where(person => person.Name.Contains(term)
                || (person.SocialName != null && person.SocialName.Contains(term))
                || person.DocumentNumber.Contains(term));
        }

        // Setor (público/privado) pela Natureza Jurídica.
        if (query.IsPrivateSector is not null)
        {
            baseQuery = baseQuery.Where(person =>
                person.LegalNature != null && person.LegalNature.IsPrivate == query.IsPrivateSector);
        }

        // Período de cadastro = data do vínculo do papel Corretor.
        if (query.RegisteredFrom is not null)
        {
            var from = query.RegisteredFrom.Value;
            baseQuery = baseQuery.Where(person => person.Roles
                .Any(role => role.Role == EPersonRole.Broker && role.CreatedAt >= from));
        }

        if (query.RegisteredTo is not null)
        {
            var to = query.RegisteredTo.Value;
            baseQuery = baseQuery.Where(person => person.Roles
                .Any(role => role.Role == EPersonRole.Broker && role.CreatedAt <= to));
        }

        // Seguradora habilitada e Motor de Cálculo: via Habilitações ativas do par.
        if (query.InsurerId is not null)
        {
            var insurerId = query.InsurerId.Value;
            var ids = enablements
                .Where(enablement => enablement.Status == EBrokerageInsurerEnablementStatus.Active
                    && enablement.InsurerId == insurerId)
                .Select(enablement => enablement.BrokerageId);
            baseQuery = baseQuery.Where(person => ids.Contains(person.Id));
        }

        if (query.CalculationEngine is not null)
        {
            var engine = query.CalculationEngine.Value;
            var ids = enablements
                .Where(enablement => enablement.Status == EBrokerageInsurerEnablementStatus.Active
                    && enablement.CalculationEngine == engine)
                .Select(enablement => enablement.BrokerageId);
            baseQuery = baseQuery.Where(person => ids.Contains(person.Id));
        }

        // Contagem por situação apresentada, sobre os demais filtros (sem a própria situação) — RN-018/RN-053.
        // O predicado é a regra única de Core (BrokerageSituationRules), a mesma que resolve a linha.
        var counts = new BrokerageSituationCountsDto(
            await baseQuery.LongCountAsync(cancellationToken),
            await baseQuery.Where(BrokerageSituationRules.Matches(EBrokerageSituation.Active)).LongCountAsync(cancellationToken),
            await baseQuery.Where(BrokerageSituationRules.Matches(EBrokerageSituation.Incomplete)).LongCountAsync(cancellationToken),
            await baseQuery.Where(BrokerageSituationRules.Matches(EBrokerageSituation.Inactive)).LongCountAsync(cancellationToken));

        var filtered = query.Situation is null
            ? baseQuery
            : baseQuery.Where(BrokerageSituationRules.Matches(query.Situation.Value));

        var totalCount = await filtered.LongCountAsync(cancellationToken);

        // Página: projeta os campos crus (a situação é resolvida em memória pela regra única, RN-053).
        // RN-018: ordena por data de cadastro (criação do papel Corretor) decrescente — as últimas
        // Corretoras cadastradas aparecem primeiro; Id (UUIDv7, monotônico) desempata.
        var pageRows = await filtered
            .OrderByDescending(person => person.Roles
                .Where(role => role.Role == EPersonRole.Broker)
                .Max(role => role.CreatedAt))
            .ThenByDescending(person => person.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(person => new
            {
                person.Id,
                person.DocumentNumber,
                person.Name,
                person.SocialName,
                IsPrivateSector = person.LegalNature == null ? (bool?)null : person.LegalNature.IsPrivate,
                Status = person.Roles.Where(role => role.Role == EPersonRole.Broker)
                    .Select(role => role.Status).First(),
                ContactEmail = person.Roles.Where(role => role.Role == EPersonRole.Broker)
                    .Select(role => role.ContactEmail).First(),
                RegisteredAt = person.Roles.Where(role => role.Role == EPersonRole.Broker)
                    .Select(role => role.CreatedAt).First(),
            })
            .ToListAsync(cancellationToken);

        var pageIds = pageRows.Select(row => row.Id).ToList();

        // Habilitações ativas dos itens da página (nome da seguradora + motor), sem N+1.
        var enablementRows = await enablements
            .Where(enablement => enablement.Status == EBrokerageInsurerEnablementStatus.Active
                && pageIds.Contains(enablement.BrokerageId))
            .Join(
                Context.Set<Insurer>().AsNoTracking(),
                enablement => enablement.InsurerId,
                insurer => insurer.Id,
                (enablement, insurer) => new
                {
                    enablement.BrokerageId,
                    Name = insurer.TradeName ?? insurer.CorporateName,
                    enablement.CalculationEngine,
                })
            .ToListAsync(cancellationToken);

        var items = pageRows
            .Select(row =>
            {
                var rows = enablementRows.Where(enablement => enablement.BrokerageId == row.Id).ToList();
                var names = rows.Select(enablement => enablement.Name).Distinct().ToList();
                var engines = rows.Select(enablement => enablement.CalculationEngine.ToString()).Distinct().ToList();

                return new BrokerageListItemDto(
                    row.Id,
                    row.DocumentNumber,
                    row.Name,
                    row.SocialName,
                    row.IsPrivateSector,
                    row.Status.ToString(),
                    BrokerageSituationRules.Resolve(row.Status, row.SocialName, row.ContactEmail).ToString(),
                    row.RegisteredAt,
                    names.Count,
                    names,
                    engines);
            })
            .ToList();

        return new BrokerageListResult(items, totalCount, counts);
    }

    public async Task<BrokerageDetailsDto?> GetBrokerageByIdAsync(
        Guid personId,
        CancellationToken cancellationToken)
    {
        var enablements = Context.Set<BrokerageInsurerEnablement>().AsNoTracking();

        var row = await Set.AsNoTracking()
            .Where(person => person.Id == personId
                && person.Type == EPersonType.J
                && person.Roles.Any(role => role.Role == EPersonRole.Broker))
            .Select(person => new
            {
                person.Id,
                person.DocumentNumber,
                person.Name,
                person.SocialName,
                LegalNatureCode = person.LegalNature == null ? null : person.LegalNature.Code,
                LegalNatureName = person.LegalNature == null ? null : person.LegalNature.Name,
                IsPrivateSector = person.LegalNature == null ? (bool?)null : person.LegalNature.IsPrivate,
                Status = person.Roles.Where(role => role.Role == EPersonRole.Broker)
                    .Select(role => role.Status).First(),
                ContactEmail = person.Roles.Where(role => role.Role == EPersonRole.Broker)
                    .Select(role => role.ContactEmail).First(),
                ContactPhone = person.Roles.Where(role => role.Role == EPersonRole.Broker)
                    .Select(role => role.ContactPhone).First(),
                ResponsibleName = person.Roles.Where(role => role.Role == EPersonRole.Broker)
                    .Select(role => role.ResponsibleName).First(),
                RegisteredAt = person.Roles.Where(role => role.Role == EPersonRole.Broker)
                    .Select(role => role.CreatedAt).First(),
                MainAddress = person.Addresses
                    .Where(address => address.IsMain)
                    .Select(address => new PersonMainAddressDto(
                        address.ZipCode,
                        address.Street,
                        address.Number,
                        address.Complement,
                        address.Neighborhood,
                        address.City,
                        address.State))
                    .FirstOrDefault(),
                EnabledInsurerCount = enablements.Count(enablement =>
                    enablement.BrokerageId == person.Id
                    && enablement.Status == EBrokerageInsurerEnablementStatus.Active),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new BrokerageDetailsDto(
            row.Id,
            row.DocumentNumber,
            row.Name,
            row.SocialName,
            row.LegalNatureCode,
            row.LegalNatureName,
            row.IsPrivateSector,
            row.Status.ToString(),
            BrokerageSituationRules.Resolve(row.Status, row.SocialName, row.ContactEmail).ToString(),
            row.ContactEmail,
            row.ContactPhone,
            row.ResponsibleName,
            row.RegisteredAt,
            row.EnabledInsurerCount,
            row.MainAddress);
    }

    public async Task<IReadOnlyList<BrokerageHistoryEventDto>> GetBrokerageHistoryAsync(
        Guid personId,
        CancellationToken cancellationToken)
    {
        // RN-055: criação e última atualização vêm do vínculo do papel Corretor. O papel guarda um
        // único UpdatedAt (last-write-wins), então não dá para distinguir com fidelidade uma mudança
        // de situação (RN-021) de uma edição de dados (RN-054) — o evento é neutro ("updated"). Um
        // evento próprio por transição exigiria trilha de auditoria/eventos (fora do escopo, RN-055).
        var role = await Context.Set<PersonRole>().AsNoTracking()
            .Where(personRole => personRole.PersonId == personId && personRole.Role == EPersonRole.Broker)
            .Select(personRole => new
            {
                personRole.CreatedAt,
                personRole.CreatedBy,
                personRole.UpdatedAt,
                personRole.UpdatedBy,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
        {
            return [];
        }

        var events = new List<BrokerageHistoryEventDto>
        {
            new("created", null, role.CreatedAt, role.CreatedBy),
        };

        if (role.UpdatedAt is not null)
        {
            events.Add(new("updated", null, role.UpdatedAt.Value, role.UpdatedBy ?? "sistema"));
        }

        // RN-055: cada Habilitação de Seguradora (criação e última alteração).
        var enablementRows = await Context.Set<BrokerageInsurerEnablement>().AsNoTracking()
            .Where(enablement => enablement.BrokerageId == personId)
            .Join(
                Context.Set<Insurer>().AsNoTracking(),
                enablement => enablement.InsurerId,
                insurer => insurer.Id,
                (enablement, insurer) => new
                {
                    Subject = insurer.TradeName ?? insurer.CorporateName,
                    enablement.CreatedAt,
                    enablement.CreatedBy,
                    enablement.UpdatedAt,
                    enablement.UpdatedBy,
                })
            .ToListAsync(cancellationToken);

        foreach (var enablement in enablementRows)
        {
            events.Add(new("insurer-enabled", enablement.Subject, enablement.CreatedAt, enablement.CreatedBy));

            if (enablement.UpdatedAt is not null)
            {
                events.Add(new(
                    "insurer-enablement-updated",
                    enablement.Subject,
                    enablement.UpdatedAt.Value,
                    enablement.UpdatedBy ?? "sistema"));
            }
        }

        return events
            .OrderByDescending(historyEvent => historyEvent.OccurredAt)
            .ToList();
    }

    public async Task<Person?> GetTrackedBrokerageByIdAsync(
        Guid personId,
        CancellationToken cancellationToken)
        => await Set
            .Include(person => person.Roles)
            .FirstOrDefaultAsync(
                person => person.Id == personId
                    && person.Type == EPersonType.J
                    && person.Roles.Any(role => role.Role == EPersonRole.Broker),
                cancellationToken);

    public async Task<BrokeragePreviewDto?> FindBrokeragePreviewByDocumentAsync(
        string documentNumber,
        CancellationToken cancellationToken)
        => await Set.AsNoTracking()
            .Where(person => person.DocumentNumber == documentNumber)
            .Select(person => new BrokeragePreviewDto(
                person.Id,
                person.DocumentNumber,
                person.Name,
                person.SocialName,
                person.LegalNature == null ? null : person.LegalNature.Code,
                person.LegalNature == null ? null : person.LegalNature.Name,
                person.LegalNature == null ? null : (bool?)person.LegalNature.IsPrivate,
                person.Roles.Any(role => role.Role == EPersonRole.Broker),
                person.Addresses
                    .Where(address => address.IsMain)
                    .Select(address => new PersonMainAddressDto(
                        address.ZipCode,
                        address.Street,
                        address.Number,
                        address.Complement,
                        address.Neighborhood,
                        address.City,
                        address.State))
                    .FirstOrDefault(),
                person.UpdatedAt ?? person.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<(IReadOnlyList<PolicyHolderListItemDto> Items, long TotalCount)> ListPolicyHoldersAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = Set.AsNoTracking()
            .Where(person => person.Type == EPersonType.J
                && person.Roles.Any(role => role.Role == EPersonRole.PolicyHolder)
                && person.DocumentNumber.Substring(8, 4) == "0001");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(person => person.Name.Contains(searchTerm)
                || (person.SocialName != null && person.SocialName.Contains(searchTerm))
                || person.DocumentNumber.Contains(searchTerm));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(person => person.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(person => new PolicyHolderListItemDto(
                person.Id,
                person.DocumentNumber,
                person.Name,
                person.SocialName,
                person.LegalNature == null ? null : (bool?)person.LegalNature.IsPrivate))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<PolicyHolderDetailsDto?> GetPolicyHolderByIdAsync(
        Guid personId,
        CancellationToken cancellationToken)
    {
        var person = await Set.AsNoTracking()
            .Where(p => p.Id == personId
                && p.Type == EPersonType.J
                && p.Roles.Any(role => role.Role == EPersonRole.PolicyHolder)
                && p.DocumentNumber.Substring(8, 4) == "0001")
            .Select(p => new
            {
                p.Id,
                p.DocumentNumber,
                p.Name,
                p.SocialName,
                LegalNatureCode = p.LegalNature == null ? null : p.LegalNature.Code,
                LegalNatureDescription = p.LegalNature == null ? null : p.LegalNature.Name,
                IsPrivateSector = p.LegalNature == null ? null : (bool?)p.LegalNature.IsPrivate,
                Addresses = p.Addresses.Select(a => new PersonAddressDetailsDto(
                    a.Id,
                    a.ZipCode,
                    a.Street,
                    a.Number,
                    a.Complement,
                    a.Neighborhood,
                    a.City,
                    a.State,
                    a.IsMain)).ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            return null;
        }

        // Single LINQ join query to avoid N+1: appointments + insurers + persons in one go
        var appointments = await Context.Set<PolicyHolderAppointment>().AsNoTracking()
            .Where(appointment => appointment.PolicyHolderId == personId)
            .OrderByDescending(appointment => appointment.StartedAt)
            .Join(Context.Set<Insurer>().AsNoTracking(),
                appointment => appointment.InsurerId,
                insurer => insurer.Id,
                (appointment, insurer) => new { appointment, insurer })
            .Join(Context.Set<Person>().AsNoTracking(),
                x => x.appointment.BrokerageId,
                broker => broker.Id,
                (x, broker) => new PolicyHolderAppointmentDetailDto(
                    x.appointment.Id,
                    x.appointment.InsurerId,
                    x.insurer.Cnpj,
                    x.insurer.CorporateName,
                    x.appointment.BrokerageId,
                    broker.DocumentNumber,
                    broker.Name,
                    x.appointment.Status.ToString(),
                    x.appointment.StartedAt,
                    x.appointment.EndedAt))
            .ToListAsync(cancellationToken);

        return new PolicyHolderDetailsDto(
            person.Id,
            person.DocumentNumber,
            person.Name,
            person.SocialName,
            person.LegalNatureCode,
            person.LegalNatureDescription,
            person.IsPrivateSector,
            person.Addresses,
            appointments);
    }

    public async Task<Person?> GetTrackedPolicyHolderByIdAsync(
        Guid personId,
        CancellationToken cancellationToken)
        => await Set
            .Include(person => person.Roles)
            .Include(person => person.Addresses)
            .FirstOrDefaultAsync(
                person => person.Id == personId
                    && person.Type == EPersonType.J
                    && person.Roles.Any(role => role.Role == EPersonRole.PolicyHolder)
                    && person.DocumentNumber.Substring(8, 4) == "0001",
                cancellationToken);

    private static IQueryable<PersonSearchItemDto> ProjectItems(IQueryable<Person> query)
        => query.Select(person => new PersonSearchItemDto(
            person.Id,
            person.DocumentNumber,
            person.Name,
            person.SocialName,
            person.Type.ToString(),
            person.LegalNature == null ? null : (bool?)person.LegalNature.IsPrivate,
            person.Roles.Select(role => role.Role.ToString()).ToList(),
            person.Addresses
                .Where(address => address.IsMain)
                .Select(address => new PersonMainAddressDto(
                    address.ZipCode,
                    address.Street,
                    address.Number,
                    address.Complement,
                    address.Neighborhood,
                    address.City,
                    address.State))
                .FirstOrDefault()));
}
