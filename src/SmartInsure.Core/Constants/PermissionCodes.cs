namespace SmartInsure.Core.Constants;

/// <summary>
/// Catálogo fixo de Permissões (RN-063, seção "Catálogo declarado — v1"). Cada funcionalidade
/// declara aqui o código que exige; o catálogo é semeado por migration (Flyway, repositório
/// dedicado) e nunca editado por tela. O Code é a chave natural — o id nunca é referenciado
/// em código. Funcionalidade que ainda não existe não declara Permissão.
/// </summary>
public static class PermissionCodes
{
    public const string QuotationGroupsView = "quotation-groups.view";
    public const string QuotationGroupsCreate = "quotation-groups.create";
    public const string QuotationGroupsEdit = "quotation-groups.edit";

    public const string CreditInquiriesView = "credit-inquiries.view";
    public const string CreditInquiriesCreate = "credit-inquiries.create";

    public const string PolicyHoldersView = "policy-holders.view";
    public const string PolicyHoldersCreate = "policy-holders.create";
    public const string PolicyHoldersEdit = "policy-holders.edit";
    public const string PolicyHolderAppointmentsManage = "policy-holder-appointments.manage";

    public const string BrokeragesView = "brokerages.view";
    public const string BrokeragesCreate = "brokerages.create";
    public const string BrokeragesEdit = "brokerages.edit";
    public const string BrokeragesChangeStatus = "brokerages.change-status";

    public const string InsurerEnablementsManage = "insurer-enablements.manage";
    public const string InsurersView = "insurers.view";

    public const string ModalitiesView = "modalities.view";
    public const string ModalitiesEdit = "modalities.edit";
    public const string ModalityMapManage = "modality-map.manage";

    public const string AdditionalCoveragesView = "additional-coverages.view";
    public const string AdditionalCoveragesEdit = "additional-coverages.edit";
    public const string AdditionalCoverageMapManage = "additional-coverage-map.manage";

    public const string ImportsRun = "imports.run";

    public const string UsersView = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersChangeActivation = "users.change-activation";

    public const string ProfilesView = "profiles.view";
    public const string ProfilesManage = "profiles.manage";

    /// <summary>O catálogo declarado — mesma lista semeada pela migration (RN-063).</summary>
    public static readonly string[] All =
    [
        QuotationGroupsView, QuotationGroupsCreate, QuotationGroupsEdit,
        CreditInquiriesView, CreditInquiriesCreate,
        PolicyHoldersView, PolicyHoldersCreate, PolicyHoldersEdit, PolicyHolderAppointmentsManage,
        BrokeragesView, BrokeragesCreate, BrokeragesEdit, BrokeragesChangeStatus,
        InsurerEnablementsManage, InsurersView,
        ModalitiesView, ModalitiesEdit, ModalityMapManage,
        AdditionalCoveragesView, AdditionalCoveragesEdit, AdditionalCoverageMapManage,
        ImportsRun,
        UsersView, UsersCreate, UsersChangeActivation,
        ProfilesView, ProfilesManage,
    ];
}
