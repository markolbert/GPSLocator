using System;

namespace J4JSoftware.InReach;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InReachInboundRequestAttribute : InReachRequestAttribute
{
    public InReachInboundRequestAttribute(
        string version,
        string svcGroup,
        string service,
        bool requiresAuthentication
    )
        :base(Direction.IPCInbound, version, svcGroup, service, requiresAuthentication)
    {
    }
}
