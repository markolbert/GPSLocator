namespace J4JSoftware.GPSLocator;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InboundAttribute : LocatorAttribute
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
