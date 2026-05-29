using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderLogistics
{
    public record UpdateOrderLogisticsCommand(
        Guid OrderId,
        Guid? CourierId,
        string? TrackingNumber,
        UpdateShippingAddressDto? ShippingAddress,
        string? DeliveryInstructions) : ICommand;

    public record UpdateShippingAddressDto(
        string? Street,
        string? City,
        string? District,
        string? State,
        string? PostalCode,
        string? Country);
}
