using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.TenantProfile.UpdateStoreAddress;

public record UpdateStoreAddressCommand(
    Address StoreAddress
) : ICommand;
