﻿namespace J4JSoftware.InReach;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InReachRequestAttribute : Attribute
{
    public InReachRequestAttribute(
        Direction direction,
        string version,
        string svcGroup,
        string service,
        bool requiresAuthentication
    )
    {
        Direction = direction;
        Version = version;
        ServiceGroup = svcGroup;
        Service = service;
        RequiresAuthentication = requiresAuthentication;
    }

    public string ServiceGroup { get; }
    public string Service { get; }
    public Direction Direction { get; }
    public string Version { get; }
    public bool RequiresAuthentication { get; }
}
