using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Common.Interfaces
{
    public interface IWhatsAppService
    {
        Task<Result<string>> UploadMediaAsync(
            byte[] fileBytes, 
            string fileName, 
            string mimeType,
            CancellationToken cancellationToken = default);

        Task<Result<string>> SendDocumentTemplateMessageAsync(
            string toPhoneNumber,
            string templateName,
            string languageCode,
            string mediaId,
            List<string> bodyVariables,
            CancellationToken cancellationToken = default);
    }
}
