using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.GetPaymentMethodsPaged
{
    public record GetPaymentMethodsPagedQuery : PagedRequest, IQuery<PagedResult<PaymentMethodConfigDto>>
    {
        public bool? IsActive { get; init; }
    }

    public record PaymentMethodConfigDto(
        Guid Id,
        PaymentMethod Type,
        string DisplayName,
        decimal FeeRate,
        bool IsActive);
}