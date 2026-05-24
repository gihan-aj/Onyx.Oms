using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.SendOrderUpdate
{
    public class SendOrderUpdateHandler : ICommandHandler<SendOrderUpdateCommand, string>
    {
        private readonly IApplicationDbContext _context;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IOrderInvoiceGenerator _invoiceGenerator;
        private readonly IShippingLabelGenerator _shippingLabelGenerator;
        private readonly ICurrentUserService _currentUserService;

        public SendOrderUpdateHandler(IApplicationDbContext context, IWhatsAppService whatsAppService, IOrderInvoiceGenerator invoiceGenerator, IShippingLabelGenerator shippingLabelGenerator, ICurrentUserService currentUserService)
        {
            _context = context;
            _whatsAppService = whatsAppService;
            _invoiceGenerator = invoiceGenerator;
            _shippingLabelGenerator = shippingLabelGenerator;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> Handle(SendOrderUpdateCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == _currentUserService.ActiveTenantId, cancellationToken);

            if (tenant == null)
                return Result.Failure<string>(Error.NotFound("Tenant.NotFound", "Tenant profile not found."));

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure<string>(Error.NotFound("Order.NotFound", "Order not found."));

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == order.CustomerId, cancellationToken);

            if (customer == null || string.IsNullOrWhiteSpace(customer.PrimaryPhone))
                return Result.Failure<string>(Error.Validation("Customer.NoPhone", "Customer does not have a valid phone number."));

            // Determine messages and document type based on OrderStatus state
            byte[] documentBytes;
            string fileName;
            string mimeType = "application/pdf";
            string updateText;
            string detailsText;

            switch (order.Status)
            {
                case OrderStatus.Confirmed:
                case OrderStatus.Processing:
                    documentBytes = _invoiceGenerator.Generate(order, customer, tenant, request.LogoStoragePath);
                    fileName = $"Document_{order.OrderNumber}.pdf"; // Generator handles Invoice vs Receipt name internally
                    updateText = "Your order has been successfully confirmed!";
                    detailsText = "Our warehouse team is already preparing your items. Full transaction details are attached.";
                    break;

                case OrderStatus.Packed:
                    documentBytes = _invoiceGenerator.Generate(order, customer, tenant, request.LogoStoragePath);
                    fileName = $"Document_{order.OrderNumber}.pdf"; // Generator handles Invoice vs Receipt name internally
                    updateText = "Your order is ready to be delivered!";
                    detailsText = "Our warehouse team has prepared your items. Full transaction details are attached.";
                    break;

                case OrderStatus.Shipped:
                    documentBytes = _shippingLabelGenerator.Generate(order, customer, tenant);
                    fileName = $"ShippingLabel_{order.OrderNumber}.pdf";
                    updateText = "Great news! Your package has been handed over to the courier.";
                    detailsText = "It is currently in transit. You can find your tracking identifier and delivery receipt details attached.";
                    break;

                case OrderStatus.Cancelled:
                    documentBytes = _invoiceGenerator.Generate(order, customer, tenant, request.LogoStoragePath);
                    fileName = $"CancellationSummary_{order.OrderNumber}.pdf";
                    updateText = "Your order has been officially cancelled.";
                    detailsText = "As requested, this order will no longer be processed. A statement of account details is attached.";
                    break;

                default:
                    return Result.Failure<string>(Error.Validation("WhatsApp.StatusNotSupported",
                        $"Sending automated notifications for status '{order.Status}' is not configured."));
            }

            var mediaResult = await _whatsAppService.UploadMediaAsync(documentBytes, fileName, mimeType, cancellationToken);
            if (mediaResult.IsFailure) 
                return Result.Failure<string>(mediaResult.Error);

            string formattedPhone = _whatsAppService.FormatPhoneNumberForWhatsApp(customer.PrimaryPhone);
            var bodyVariables = new List<string>
            {
                customer.Name,          // {{1}} Hello Name
                updateText,             // {{2}} Update Block
                detailsText,            // {{3}} Details Block
                order.OrderNumber       // {{4}} Reference ID
            };

            return await _whatsAppService.SendDocumentTemplateMessageAsync(
                toPhoneNumber: formattedPhone,
                templateName: "oms_transactional_update",
                languageCode: "en_US",
                mediaId: mediaResult.Value,
                fileName: fileName,
                bodyVariables: bodyVariables,
                cancellationToken: cancellationToken);
        }
    }
}
