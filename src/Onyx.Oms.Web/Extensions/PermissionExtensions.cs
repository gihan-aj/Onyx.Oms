using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Onyx.Oms.Infrastructure.Identity;

namespace Onyx.Oms.Web.Extensions;

public static class PermissionExtensions
{
    public static RouteHandlerBuilder HasPermission(this RouteHandlerBuilder builder, string permission)
    {
        return builder.RequireAuthorization(permission);
    }
}
