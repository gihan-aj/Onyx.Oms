namespace Onyx.Oms.Core.Common.Interfaces;

public interface IPermissionService
{
    Task<HashSet<string>?> GetPermissionsAsync(Guid userId);
}
