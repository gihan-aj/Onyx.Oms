using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.WhatsApp.UpdateWhatsAppSettings
{
    public class UpdateWhatsAppSettingsCommandHandler : ICommandHandler<UpdateWhatsAppSettingsCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICryptoService _cryptoService;

        public UpdateWhatsAppSettingsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, ICryptoService cryptoService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _cryptoService = cryptoService;
        }

        public async Task<Result> Handle(UpdateWhatsAppSettingsCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _context.Tenants
                .Include(t => t.WhatsAppSettings)
                .FirstOrDefaultAsync(t => t.Id == _currentUserService.ActiveTenantId, cancellationToken);

            if (tenant == null)
                return Result.Failure(Error.NotFound("Tenant.NotFound", "Current tenant context not found."));

            string encryptedTokenToSave;

            // Scenario A: User provided a new token
            if (!string.IsNullOrWhiteSpace(request.AccessToken))
            {
                encryptedTokenToSave = _cryptoService.Encrypt(request.AccessToken);
            }
            // Scenario B: User left token blank, keep the old one
            else if (tenant.WhatsAppSettings != null)
            {
                encryptedTokenToSave = tenant.WhatsAppSettings.EncryptedAccessToken;
            }
            // Scenario C: First time setup, but they didn't provide a token
            else
            {
                return Result.Failure(Error.Validation("WhatsApp.TokenRequired", "An Access Token is required for initial setup."));
            }

            var updateResult = tenant.ConfigureWhatsAppSettings(request.PhoneNumberId, encryptedTokenToSave);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            var whatsAppSettings = updateResult.Value;
            if (whatsAppSettings != null)
                _context.TenantWhatsAppSettings.Add(whatsAppSettings);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
