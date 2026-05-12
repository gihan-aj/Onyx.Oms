using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateStoreInfo;

public record UpdateStoreInfoCommand(
    string StoreName,
    string? LegalName,
    string? TaxRegistrationNumber,
    string ContactEmail,
    string? ContactPhone,
    string? InvoiceFooterText
) : ICommand;
