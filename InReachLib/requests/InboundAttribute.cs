using System;

namespace J4JSoftware.InReach;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InboundAttribute : InReachAttribute
{
    public InboundAttribute(
        string version,
        string svcGroup,
        string service,
        bool requiresAuthentication
    )
        :base(Direction.IPCInbound, version, svcGroup, service, requiresAuthentication)
    {
    }
}
