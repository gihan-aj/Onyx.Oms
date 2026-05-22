using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.SendOrderConfirmation
{
    public class SendOrderConfirmationHandler : ICommandHandler<SendOrderConfirmationCommand, string>
    {
        private readonly IApplicationDbContext _context;
        private readonly IWhatsAppService _whatsAppService;

        public SendOrderConfirmationHandler(IApplicationDbContext context, IWhatsAppService whatsAppService)
        {
            _context = context;
            _whatsAppService = whatsAppService;
        }

        public async Task<Result<string>> Handle(SendOrderConfirmationCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure<string>(Error.NotFound("Order.NotFound", "Order not found."));

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == order.CustomerId, cancellationToken);

            if (customer == null)
                return Result.Failure<string>(Error.NotFound("Customer.NotFound", "Customer not found."));

            if (string.IsNullOrWhiteSpace(customer.PrimaryPhone))
                return Result.Failure<string>(Error.Validation("Customer.NoPhone", "Customer does not have a primary phone number."));

            if (order.Status == Core.Domain.Enums.OrderStatus.Pending)
                return Result.Failure<string>(Error.Validation("Order.Pending", "Cannot send confirmation for a pending order."));

            string formattedPhone = FormatPhoneNumberForWhatsApp(customer.PrimaryPhone);

            var messageResult = await _whatsAppService.SendTemplateMessageAsync(
                toPhoneNumber: formattedPhone,
                templateName: "hello_world",
                languageCode: "en_US",
                cancellationToken: cancellationToken);

            if (messageResult.IsFailure)
                return Result.Failure<string>(messageResult.Error);

            return Result.Success(messageResult.Value);
        }

        /// <summary>
        /// Formats a local Sri Lankan number (e.g., 0771234567) into the international format (94771234567) required by Meta.
        /// </summary>
        private string FormatPhoneNumberForWhatsApp(string rawPhone)
        {
            var digitsOnly = new string(rawPhone.Where(char.IsDigit).ToArray());
            
            // If is starts with 0 and is 10 digits long (Standard LK format)
            if(digitsOnly.StartsWith("0") && digitsOnly.Length == 10)
            {
                return "94" + digitsOnly.Substring(1);
            }

            if(digitsOnly.StartsWith("94") &&  digitsOnly.Length == 11)
            {
                return digitsOnly;
            }

            return rawPhone;
        }
    }
}
