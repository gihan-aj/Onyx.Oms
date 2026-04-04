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
                await currentUserService.GetActiveTenantIdAsync(httpContext.RequestAborted);
            }

            await _next(httpContext);
        }
    }
}
