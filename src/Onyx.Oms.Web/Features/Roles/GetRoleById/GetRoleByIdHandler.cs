using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.GetRoleById;

public class GetRoleByIdHandler : IQueryHandler<GetRoleByIdQuery, RoleDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetRoleByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RoleDetailDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
        {
            return Result.Failure<RoleDetailDto>(Error.NotFound("Role.NotFound", $"Role with Id {request.Id} was not found."));
        }

        var dto = new RoleDetailDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.Permissions.ToList()
        );

        return Result.Success(dto);
    }
}
