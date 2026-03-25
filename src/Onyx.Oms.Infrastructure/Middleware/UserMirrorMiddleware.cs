using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Infrastructure.Middleware;

public class UserMirrorMiddleware
{
    //private readonly RequestDelegate _next;
    //private readonly ILogger<UserMirrorMiddleware> _logger;

    //public UserMirrorMiddleware(RequestDelegate next, ILogger<UserMirrorMiddleware> logger)
    //{
    //    _next = next;
    //    _logger = logger;
    //}

    //public async Task InvokeAsync(HttpContext context, IApplicationDbContext db)
    //{
    //    if (context.User.Identity?.IsAuthenticated == true)
    //    {
    //        var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
    //                  ?? context.User.FindFirst("sub")?.Value;

    //        if (!string.IsNullOrEmpty(sub))
    //        {
    //            // TODO: Add caching layer here to avoid hitting DB on every request
    //            // For now, checks DB directly

    //            var user = await db.AppUsers
    //                .Include(u => u.Roles) // Important: Include roles to update them
    //                .FirstOrDefaultAsync(u => u.IdentityUserId == sub);

    //            bool rolesChanged = false;

    //            if (user == null)
    //            {
    //                _logger.LogInformation("Creating local user mirror for IdentityUserId: {UserId}", sub);
                    
    //                // Create new user using Factory method
    //                var email = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "";
    //                var name = context.User.FindFirst(ClaimTypes.Name)?.Value ?? email;
                    
    //                user = AppUser.Create(sub, email, name);
                    
    //                db.AppUsers.Add(user);
    //                await db.SaveChangesAsync(CancellationToken.None); // Save to get an ID before role assignment if needed
    //            }
    //            else
    //            {
    //                // Update valid user login time
    //                if (user.LastLoginUtc == null || user.LastLoginUtc < DateTime.UtcNow.AddMinutes(-5))
    //                {
    //                    user.UpdateLastLoginTime();
    //                }
    //            }

    //            // --- Role Synchronization Logic ---
    //            // Extract role claims from the current token
    //            var tokenRoleClaims = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                
    //            // Also look for "role" literally, depending on how JWT is mapped
    //            if (!tokenRoleClaims.Any())
    //            {
    //                tokenRoleClaims = context.User.FindAll("role").Select(c => c.Value).ToList();
    //            }

    //            if (tokenRoleClaims.Any())
    //            {
    //                // Find matching local roles by Name (IdP role names match local Role.Name exactly)
    //                 var localRolesToAssign = await db.Roles
    //                    .Where(r => tokenRoleClaims.Contains(r.Name))
    //                    .ToListAsync();

    //                // 1. Assign missing roles
    //                foreach (var localRole in localRolesToAssign)
    //                {
    //                    if (!user.Roles.Any(r => r.Id == localRole.Id))
    //                    {
    //                        user.AssignRole(localRole);
    //                        rolesChanged = true;
    //                        _logger.LogInformation("Assigned role {RoleName} to user {UserId} via token sync.", localRole.Name, user.IdentityUserId);
    //                    }
    //                }

    //                // 2. Remove obsolete roles (roles user has locally but are NOT in the current token)
    //                // Note: If you want to allow purely local roles (not in IdP), remove this block.
    //                // But to prevent split-brain and ensure IdP is the source of truth for Role ASSIGNMENTS, we must sync deletions too.
    //                var rolesToRemove = user.Roles.Where(ur => !tokenRoleClaims.Contains(ur.Name)).ToList();
    //                foreach (var roleToRemove in rolesToRemove)
    //                {
    //                    user.RemoveRole(roleToRemove);
    //                    rolesChanged = true;
    //                    _logger.LogInformation("Removed role {RoleName} from user {UserId} via token sync.", roleToRemove.Name, user.IdentityUserId);
    //                }
    //            }

    //            // If anything changed (login time or roles), save it.
    //            if (rolesChanged || db.ChangeTracker.HasChanges())
    //            {
    //                await db.SaveChangesAsync(CancellationToken.None);
    //            }
    //        }
    //        else
    //        {
    //            _logger.LogWarning("User is authenticated but 'sub' or 'NameIdentifier' claim is missing.");
    //        }
    //    }
    //    else
    //    {
    //        _logger.LogDebug("User is NOT authenticated. Headers: {Headers}", 
    //            string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}")));
    //    }

    //    await _next(context);
    //}
}
