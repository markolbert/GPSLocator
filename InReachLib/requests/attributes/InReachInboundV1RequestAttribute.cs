namespace J4JSoftware.InReach;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InReachInboundV1RequestAttribute : InReachInboundRequestAttribute
{
    public InReachInboundV1RequestAttribute(
        string svcGroup,
        string service,
        bool requiresAuthentication
    )
        : base("1", svcGroup, service, requiresAuthentication)
    {
    }
}
