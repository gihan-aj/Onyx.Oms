using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.GetCustomersPaged;

public class GetCustomersPagedHandler : IQueryHandler<GetCustomersPagedQuery, PagedResult<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomersPagedHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<CustomerDto>>> Handle(GetCustomersPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers.AsNoTracking();

        // 1. Filtering
        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // Search in Name, Email, or Phone
            query = query.Where(c => 
                c.Name.Contains(request.SearchTerm) ||
                (c.Email != null && c.Email.Contains(request.SearchTerm)) || 
                c.PrimaryPhone.Contains(request.SearchTerm) || 
                (c.LastOrderNumber != null && c.LastOrderNumber.Contains(request.SearchTerm)));
        }

        // 2. Sorting
        query = ApplySorting(query, request.SortColumn, request.SortOrder);

        // 3. Projections
        var dtoQuery = query.Select(c => new CustomerDto(
            c.Id,
            c.Name,
            c.Email,
            c.PrimaryPhone,
            c.SecondaryPhone,
            c.LastOrderNumber,
            c.Address,
            c.Notes,
            c.DeliveryInstructions,
            c.IsActive,
            c.CreatedOnUtc));

        // 4. Pagination
        var pagedResult = await PagedResult<CustomerDto>.CreateAsync(dtoQuery, request.Page, request.PageSize, cancellationToken);

        return Result.Success(pagedResult);
    }

    private static IQueryable<Core.Domain.Entities.Customer> ApplySorting(IQueryable<Core.Domain.Entities.Customer> query, string? sortColumn, string? sortOrder)
    {
        bool isDesc = sortOrder?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            return isDesc ? query.OrderByDescending(c => c.CreatedOnUtc) : query.OrderByDescending(c => c.CreatedOnUtc);
        }

        return sortColumn.ToLower() switch
        {
            "name" => isDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "email" => isDesc ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "primaryphone" => isDesc ? query.OrderByDescending(c => c.PrimaryPhone) : query.OrderBy(c => c.PrimaryPhone),
            "city" => isDesc ? query.OrderByDescending(c => c.Address.City) : query.OrderBy(c => c.Address.City),
            "lastordernumber" => isDesc ? query.OrderByDescending(c => c.LastOrderNumber) : query.OrderBy(c => c.LastOrderNumber),
            "isactive" => isDesc ? query.OrderByDescending(c => c.IsActive) : query.OrderBy(c => c.IsActive),
            "createddate" => isDesc ? query.OrderByDescending(c => c.CreatedOnUtc) : query.OrderBy(c => c.CreatedOnUtc),
            _ => query.OrderByDescending(c => c.CreatedOnUtc)
        };
    }
}
