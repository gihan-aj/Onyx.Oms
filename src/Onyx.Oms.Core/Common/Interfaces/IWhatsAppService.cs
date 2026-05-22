using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Common.Interfaces
{
    public interface IWhatsAppService
    {
        Task<Result<string>> SendTemplateMessageAsync(
            string toPhoneNumber, 
            string templateName, 
            string languageCode, 
            CancellationToken cancellationToken = default);
    }
}
