using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.GetRoles;

public class GetRolesHandler : IQueryHandler<GetRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRolesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Roles.AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var roles = await query
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(
                r.Id, 
                r.Name, 
                r.Description, 
                r.Permissions.Count,
                r.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<RoleDto>>(roles);
    }
}
