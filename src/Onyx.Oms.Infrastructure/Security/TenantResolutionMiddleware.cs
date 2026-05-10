using Microsoft.AspNetCore.Http;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Infrastructure.Identity;

namespace Onyx.Oms.Infrastructure.Security
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, ICurrentUserService currentUserService)
        {
            if (currentUserService.IsAuthenticated)
            {
                try
                {
                    await currentUserService.GetActiveTenantIdAsync(httpContext.RequestAborted);
                }
                catch (Exception ex)
                {
                    // Log the exception
                    httpContext.Items["TenantResolutionError"] = ex.Message;
                    // Optionally return 401 or let it continue
                    // return;
                }
            }

            await _next(httpContext);
        }
    }
}
