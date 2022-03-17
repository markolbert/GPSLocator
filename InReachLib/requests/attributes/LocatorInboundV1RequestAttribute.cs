namespace J4JSoftware.InReach;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LocatorInboundV1RequestAttribute : LocatorInboundRequestAttribute
{
    public LocatorInboundV1RequestAttribute(
        string svcGroup,
        string service,
        bool requiresAuthentication
    )
        : base("1", svcGroup, service, requiresAuthentication)
    {
    }
}
