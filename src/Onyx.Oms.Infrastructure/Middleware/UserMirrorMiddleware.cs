using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Middleware;

public class UserMirrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserMirrorMiddleware> _logger;

    public UserMirrorMiddleware(RequestDelegate next, ILogger<UserMirrorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IApplicationDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                      ?? context.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(sub))
            {
                // TODO: Add caching layer here to avoid hitting DB on every request
                // For now, checks DB directly

                var user = await db.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == sub);

                if (user == null)
                {
                    _logger.LogInformation("Creating local user mirror for IdentityUserId: {UserId}", sub);
                    
                    // Create new user using Factory method
                    var email = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                    var name = context.User.FindFirst(ClaimTypes.Name)?.Value ?? email;
                    
                    user = AppUser.Create(sub, email, name);
                    
                    db.AppUsers.Add(user);
                    await db.SaveChangesAsync(CancellationToken.None);
                }
                else
                {
                    // Update valid user login time
                    if (user.LastLoginUtc == null || user.LastLoginUtc < DateTime.UtcNow.AddMinutes(-5))
                    {
                        user.RecordLogin();
                        await db.SaveChangesAsync(CancellationToken.None);
                    }
                }
            }
            else
            {
                _logger.LogWarning("User is authenticated but 'sub' or 'NameIdentifier' claim is missing.");
            }
        }
        else
        {
            _logger.LogDebug("User is NOT authenticated. Headers: {Headers}", 
                string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}")));
        }

        await _next(context);
    }
}
