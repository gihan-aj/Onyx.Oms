using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.GetCouriersPaged;

public class GetCouriersPagedHandler : IQueryHandler<GetCouriersPagedQuery, PagedResult<CourierDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCouriersPagedHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<CourierDto>>> Handle(GetCouriersPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Couriers.AsNoTracking();

        // 1. Filtering
        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // Simple robust search: check Name
            // If we had "SearchIn" logic we could expand this.
            query = query.Where(c => c.Name.Contains(request.SearchTerm));
        }

        // 2. Sorting
        // Basic implementation without dynamic linq library
        query = ApplySorting(query, request.SortColumn, request.SortOrder);

        // 3. Projections
        var dtoQuery = query.Select(c => new CourierDto(
                c.Id,
                c.Name,
                c.ContactPerson,
                c.PrimaryPhone,
                c.SecondaryPhone,
                c.WebsiteUrl,
                c.IsActive));

        // 4. Pagination
        var pagedResult = await PagedResult<CourierDto>.CreateAsync(dtoQuery, request.Page, request.PageSize, cancellationToken);

        return Result.Success(pagedResult);
    }

    private static IQueryable<Core.Domain.Entities.Courier> ApplySorting(IQueryable<Core.Domain.Entities.Courier> query, string? sortColumn, string? sortOrder)
    {
        bool isDesc = sortOrder?.ToLower() == "desc";

        // Default sort
        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            return isDesc ? query.OrderByDescending(c => c.CreatedOnUtc) : query.OrderByDescending(c => c.CreatedOnUtc); // Default to newest
        }

        return sortColumn.ToLower() switch
        {
            "name" => isDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "contactperson" => isDesc ? query.OrderByDescending(c => c.ContactPerson) : query.OrderBy(c => c.ContactPerson),
            "isactive" => isDesc ? query.OrderByDescending(c => c.IsActive) : query.OrderBy(c => c.IsActive),
            "createddate" => isDesc ? query.OrderByDescending(c => c.CreatedOnUtc) : query.OrderBy(c => c.CreatedOnUtc),
            _ => query.OrderByDescending(c => c.CreatedOnUtc)
        };
    }
}
