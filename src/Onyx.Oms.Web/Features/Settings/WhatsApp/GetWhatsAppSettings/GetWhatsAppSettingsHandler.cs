using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.WhatsApp.GetWhatsAppSettings
{
    public class GetWhatsAppSettingsHandler : IQueryHandler<GetWhatsAppSettingsQuery, WhatsAppSettingsDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetWhatsAppSettingsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<WhatsAppSettingsDto>> Handle(GetWhatsAppSettingsQuery request, CancellationToken cancellationToken)
        {
            var settings = await _context.Tenants
                .AsNoTracking()
                .Where(t => t.Id == _currentUserService.ActiveTenantId)
                .Select(t => t.WhatsAppSettings)
                .FirstOrDefaultAsync(cancellationToken);

            if (settings == null)
                return new WhatsAppSettingsDto(null, false);

            return new WhatsAppSettingsDto(settings.PhoneNumberId, !string.IsNullOrWhiteSpace(settings.EncryptedAccessToken));
        }
    }
}
