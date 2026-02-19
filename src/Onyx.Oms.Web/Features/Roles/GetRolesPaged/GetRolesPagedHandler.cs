using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.GetRolesPaged;

public class GetRolesPagedHandler : IQueryHandler<GetRolesPagedQuery, PagedResult<RoleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRolesPagedHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<RoleDto>>> Handle(GetRolesPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Roles.AsNoTracking();

        // 1. Filtering
        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(r => r.Name.Contains(request.SearchTerm));
        }

        // 2. Sorting
        query = ApplySorting(query, request.SortColumn, request.SortOrder);

        // 3. Projections
        var dtoQuery = query.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.Permissions.Count,
            r.Users.Count,
            r.IsActive));

        // 4. Pagination
        var pagedResult = await PagedResult<RoleDto>.CreateAsync(dtoQuery, request.Page, request.PageSize, cancellationToken);

        return Result.Success(pagedResult);
    }

    private static IQueryable<Core.Domain.Entities.Role> ApplySorting(IQueryable<Core.Domain.Entities.Role> query, string? sortColumn, string? sortOrder)
    {
        bool isDesc = sortOrder?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            return isDesc ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name);
        }

        return sortColumn.ToLower() switch
        {
            "name" => isDesc ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            "description" => isDesc ? query.OrderByDescending(r => r.Description) : query.OrderBy(r => r.Description),
            "permissioncount" => isDesc ? query.OrderByDescending(r => r.Permissions.Count) : query.OrderBy(r => r.Permissions.Count),
            "usercount" => isDesc ? query.OrderByDescending(r => r.Users.Count) : query.OrderBy(r => r.Users.Count),
            "isactive" => isDesc ? query.OrderByDescending(r => r.IsActive) : query.OrderBy(r => r.IsActive),
            _ => query.OrderBy(r => r.Name)
        };
    }
}
