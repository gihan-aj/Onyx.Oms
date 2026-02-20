using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Users.GetCurrentUserPermissions;

public record GetCurrentUserPermissionsQuery() : IQuery<List<string>>;
