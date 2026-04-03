using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Infrastructure.Identity;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Onyx.Oms.Infrastructure.Persistence;
using Onyx.Oms.Infrastructure.Persistence.Interceptors;
using Onyx.Oms.Infrastructure.Persistence.Seeding;
using Onyx.Oms.Infrastructure.Services;
using Refit;

namespace Onyx.Oms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAppSequenceService, AppSequenceService>();

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<TenantSecurityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddMemoryCache(); // Required for caching decorator

        // Permission Services
        services.AddScoped<PermissionService>();
        services.AddScoped<IPermissionService>(provider =>
        {
            var innerService = provider.GetRequiredService<PermissionService>();
            var cache = provider.GetRequiredService<IMemoryCache>();

            return new CachedPermissionService(innerService, cache);
        });

        services.AddAuthorization();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // IdP Services
        services.AddTransient<IdPTokenHandler>();

        services.AddRefitClient<IIdentityProviderApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthenticationOptions>>().Value;
                client.BaseAddress = new Uri(options.Authority);
            })
            .AddHttpMessageHandler<IdPTokenHandler>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var tenantSecurityInterceptor = sp.GetRequiredService<TenantSecurityInterceptor>();
            var domainEventsInterceptor = sp.GetRequiredService<DispatchDomainEventsInterceptor>();

            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .AddInterceptors(auditableInterceptor, tenantSecurityInterceptor, domainEventsInterceptor);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<SubscriptionPlanSeeder>();

        // Authentication Configuration
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
        
        var authOptions = configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>();

        if (authOptions != null)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = authOptions.Authority;
                options.Audience = authOptions.Audience;
                options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidAudience = authOptions.Audience,
                    ValidIssuer = authOptions.Authority
                };
            });
        }

        return services;
    }
}
