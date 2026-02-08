using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.GetCustomersPaged;

public record GetCustomersPagedQuery : PagedRequest, IQuery<PagedResult<CustomerDto>>
{
    public bool? IsActive { get; init; }
}
