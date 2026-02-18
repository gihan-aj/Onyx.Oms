namespace Onyx.Oms.Core.Common.Interfaces;

public interface IPermissionService
{
    Task<HashSet<string>?> GetPermissionsAsync(int userId);
}
