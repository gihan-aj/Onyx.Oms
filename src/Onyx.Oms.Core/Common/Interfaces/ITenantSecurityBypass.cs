namespace Onyx.Oms.Core.Common.Interfaces
{
    public interface ITenantSecurityBypass
    {
        bool IsBypassEnabled { get; }
        IDisposable EnableBypass();
    }
}
