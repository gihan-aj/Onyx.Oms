using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Infrastructure.Identity;
using Onyx.Oms.Infrastructure.Identity.IdP;
using Onyx.Oms.Infrastructure.Persistence;
using Onyx.Oms.Infrastructure.Persistence.Interceptors;
using Onyx.Oms.Infrastructure.Persistence.Seeding;
using Refit;

namespace Onyx.Oms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditableEntityInterceptor>();

        // Permission Services
        services.AddScoped<PermissionService>();
        services.AddScoped<IPermissionService>(provider => 
            new CachedPermissionService(
                provider.GetRequiredService<PermissionService>(),
                provider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>()));

        services.AddMemoryCache(); // Required for caching decorator

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // IdP Services
        services.AddTransient<IdPTokenHandler>();

        services.AddRefitClient<IIdentityProviderApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthenticationOptions>>().Value;
                client.BaseAddress = new Uri(options.Authority);
            })
            .AddHttpMessageHandler<IdPTokenHandler>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<PermissionSeeder>();

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

        services.AddAuthorization();

        return services;
    }
}
