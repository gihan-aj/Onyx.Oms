using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.WhatsApp.TestConnection
{
    public class TestWhatsAppConnectionHandler : ICommandHandler<TestWhatsAppConnectionCommand, string>
    {
        private readonly IWhatsAppService _whatsAppService;

        public TestWhatsAppConnectionHandler(IWhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        public async Task<Result<string>> Handle(TestWhatsAppConnectionCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DestinationPhone))
                return Result.Failure<string>(Error.Validation("Test.PhoneRequired", "A destination phone number is required to run a diagnostic ping."));

            var formattedPhone = _whatsAppService.FormatPhoneNumberForWhatsApp(request.DestinationPhone);

            // Build a small text mock byte-array file to act as the sample document attachment
            byte[] diagnosticPdfBytes = System.Text.Encoding.UTF8.GetBytes("Onyx OMS Diagnostic Connection Verification Document.");

            // Send file up to Meta's server
            string fileName = "DiagnosticPing.txt";
            var mediaResult = await _whatsAppService.UploadMediaAsync(diagnosticPdfBytes, fileName, "text/plain", cancellationToken);
            if (mediaResult.IsFailure) 
                return Result.Failure<string>(mediaResult.Error);

            // Fire the generic template with structural dummy strings
            var textVariables = new List<string>
            {
                "System Administrator",                // {{1}} Hello Name
                "Integration Connectivity Diagnostic",   // {{2}} Update
                "Outgoing text capability validated.",  // {{3}} Details
                "PING-TEST-OK"                          // {{4}} Reference ID
            };

            var messageResult = await _whatsAppService.SendDocumentTemplateMessageAsync(
                toPhoneNumber: formattedPhone,
                templateName: "oms_transactional_update",
                languageCode: "en_US",
                mediaId: mediaResult.Value,
                fileName: fileName,
                bodyVariables: textVariables,
                cancellationToken: cancellationToken);

            if (messageResult.IsFailure) 
                return Result.Failure<string>(messageResult.Error);

            return Result.Success("Diagnostic message dispatched successfully. Check your handset.");
        }
    }
}
