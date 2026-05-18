using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Infrastructure.Identity.IdP;

namespace Onyx.Oms.Web.Features.Users.RegisterUser;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityProviderApi _idpApi;
    private readonly IAppSequenceService _appSequenceService;

    public RegisterUserHandler(IApplicationDbContext context, IIdentityProviderApi idpApi, IAppSequenceService appSequenceService)
    {
        _context = context;
        _idpApi = idpApi;
        _appSequenceService = appSequenceService;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Get the requested SubscriptionPlan
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == request.SubscriptionDetails.SubscriptionId, cancellationToken);
        if (plan == null)
            return Result.Failure<Guid>(Error.NotFound("SubscriptionPlan.NotFound", "The selected subscription plan was not found."));

        // Validate "Admin" role exists locally
        var tenantOwnerRole = await _context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Name == Core.Domain.Constants.Roles.Oms.TenantOwner, cancellationToken);
        if (tenantOwnerRole == null)
            return Result.Failure<Guid>(Error.NotFound("Role.NotFound", "The Tenant Owner role could not be found. Please ensure it is seeded."));

        // Create Tenant
        var tenantResult = Tenant.Create(request.CompanyDetails.CompanyName, request.CompanyDetails.ContactEmail, null);
        if (tenantResult.IsFailure)
            return Result.Failure<Guid>(tenantResult.Error);

        var tenant = tenantResult.Value;

        var defaultPaymentConfigs = DefaultPaymentMethods.GetConfigs(tenant.Id);
        _context.PaymentMethodConfigs.AddRange(defaultPaymentConfigs);

        // Create TenantSubscription
        var trialEnd = plan.TrialPeriodInDays > 0 ? DateTimeOffset.UtcNow.AddDays(plan.TrialPeriodInDays) : (DateTimeOffset?)null;
        
        var subscriptionResult = TenantSubscription.Create(
            tenant.Id, 
            Guid.NewGuid(), 
            plan, 
            trialEnd
        );
        if (subscriptionResult.IsFailure)
            return Result.Failure<Guid>(subscriptionResult.Error);

        tenant.SetSubscription(subscriptionResult.Value);
        _context.Tenants.Add(tenant);

        // Register User in IdP
        Guid identityUserId;
        try
        {
            var registerRequest = new RegisterUserRequest(
                request.UserDetails.FirstName,
                request.UserDetails.LastName,
                request.UserDetails.Email,
                request.UserDetails.Password,
                tenant.Id
            );

            var idpResponse = await _idpApi.RegisterUserAsync(registerRequest);
            
            if (!idpResponse.IsSuccessStatusCode || idpResponse.Content == null)
            {
                return Result.Failure<Guid>(Error.Failure("Identity.RegistrationFailed", $"Failed to register user in Identity Provider. Status: {idpResponse.StatusCode}. Message: {idpResponse.Error?.Content}"));
            }

            identityUserId = idpResponse.Content.UserId;
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Failure("Identity.Connection", $"Failed to connect to IdP: {ex.Message}"));
        }

        // Create AppUser locally
        var appUserResult = AppUser.Create(identityUserId, tenant.Id, request.UserDetails.Email, request.UserDetails.FirstName, request.UserDetails.LastName);
        if (appUserResult.IsFailure)
            return Result.Failure<Guid>(appUserResult.Error);

        var appUser = appUserResult.Value;
        
        // Assign Admin role locally
        var roleAssignResult = appUser.AssignRole(tenantOwnerRole);
        if (roleAssignResult.IsFailure)
            return Result.Failure<Guid>(roleAssignResult.Error);

        tenant.AddUser(appUser);
        _context.AppUsers.Add(appUser);

        // Create sequences for "PROD" and "ORD"
        _appSequenceService.InitialzeDefaultSequences(tenant.Id);

        // Commit all changes
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(appUser.Id);
    }
}
