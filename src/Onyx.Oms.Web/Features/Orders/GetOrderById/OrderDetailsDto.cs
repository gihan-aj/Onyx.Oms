using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Web.Features.Orders.GetOrderById
{
    public record OrderDetailsDto(
        Guid Id,
        string OrderNumber,
        CustomerDetailsDto Customer,
        Guid? CourierId,
        string? TrackingNumber,
        string ShippingAddressStreet,
        string ShippingAddressCity,
        string ShippingAddressDistrict,
        string ShippingAddressState,
        string ShippingAddressPostalCode,
        string ShippingAddressCountry,
        OrderStatus Status,
        PaymentStatus PaymentStatus,
        bool IsCashOnDelivery,
        string? DeliveryInstructions,
        string? Notes,
        decimal SubTotal,
        decimal DiscountAmount,
        string? DiscountReason,
        decimal ShippingCost,
        decimal TaxAmount,
        decimal GrandTotal,
        decimal TotalPaid,
        decimal BalanceAmount,
        string BaseCurrency,
        DateTimeOffset? OrderDate,
        DateTimeOffset CreatedOnUtc,
        List<OrderItemDetailsDto> Items,
        List<OrderPaymentDetailsDto> Payments
    );

    public record CustomerDetailsDto(
        Guid Id,
        string Name,
        string PrimaryPhone,
        string? SecondaryPhone,
        string? Email,
        Address Address,
        string? LastOrderNumber,
        string? DeliveryInstructions,
        string? Notes
    );

    public record OrderItemDetailsDto(
        Guid Id,
        Guid ProductVariantId,
        string ProductName,
        string Sku,
        string? ImageUrl,
        int AvailableQuantity,
        int Quantity,
        int AllocatedQuantity,
        int PendingQuantity,
        int IncomingStock,
        decimal unitWeight,
        string weightUnit,
        decimal UnitPrice,
        decimal DiscountAmount,
        string? DiscountReason,
        decimal LineTotal,
        OrderItemStatus Status
    );

    public record OrderPaymentDetailsDto(
        Guid Id,
        decimal Amount,
        PaymentMethod Method,
        string? Reference,
        DateTimeOffset PaymentDate,
        string? GatewayName,
        string? GatewayTransactionId,
        string? GatewayPaymentStatus
    );
}
