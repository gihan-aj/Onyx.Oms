using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Customers.GetCustomerById;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerDto>(Error.NotFound("Customer.NotFound", "Customer not found."));
        }

        var dto = new CustomerDto(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.PrimaryPhone,
            customer.SecondaryPhone,
            customer.Address,
            customer.Notes,
            customer.IsActive,
            customer.CreatedOnUtc);

        return Result.Success(dto);
    }
}
