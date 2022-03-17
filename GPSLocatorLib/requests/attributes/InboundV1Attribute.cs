namespace J4JSoftware.GPSLocator;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InboundV1Attribute : InboundAttribute
{
    public InboundV1Attribute(
        string svcGroup,
        string service,
        bool requiresAuthentication
    )
        : base("1", svcGroup, service, requiresAuthentication)
    {
    }
}
