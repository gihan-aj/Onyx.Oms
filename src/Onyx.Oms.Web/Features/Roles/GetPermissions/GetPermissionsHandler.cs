using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Roles.GetPermissions;

public class GetPermissionsHandler : IQueryHandler<GetPermissionsQuery, List<PermissionGroupDto>>
{
    public Task<Result<List<PermissionGroupDto>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var groups = new List<PermissionGroupDto>();
        var rootType = typeof(Permissions);

        foreach (var nestedType in rootType.GetNestedTypes(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        {
            var permissionsInGroup = new List<PermissionDto>();
            
            foreach (var field in nestedType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy))
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    if (field.GetValue(null) is string value)
                    {
                        permissionsInGroup.Add(new PermissionDto(field.Name, value));
                    }
                }
            }

            if (permissionsInGroup.Any())
            {
                groups.Add(new PermissionGroupDto(nestedType.Name, permissionsInGroup));
            }
        }

        // Return sorted by GroupName
        return Task.FromResult(Result.Success(groups.OrderBy(g => g.GroupName).ToList()));
    }
}
