using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Infrastructure.Security
{
    public class TenantSecurityBypass : ITenantSecurityBypass
    {
        public bool IsBypassEnabled { get; private set; }

        public IDisposable EnableBypass()
        {
            IsBypassEnabled = true;
            return new BypassScope(this);
        }

        private class BypassScope : IDisposable
        {
            private readonly TenantSecurityBypass _bypass;

            public BypassScope(TenantSecurityBypass bypass) => _bypass = bypass;

            // This automatically turns the security back ON when the using block ends
            public void Dispose() => _bypass.IsBypassEnabled = false;
        }
    }
}
