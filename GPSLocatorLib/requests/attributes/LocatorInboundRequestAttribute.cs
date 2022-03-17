namespace J4JSoftware.GPSLocator;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LocatorInboundRequestAttribute : LocatorRequestAttribute
{
    public LocatorInboundRequestAttribute(
        string version,
        string svcGroup,
        string service,
        bool requiresAuthentication
    )
        :base(Direction.IPCInbound, version, svcGroup, service, requiresAuthentication)
    {
    }
}
