using System;

namespace J4JSoftware.GPSCommon;

public class MapServiceAttribute : Attribute
{
    public MapServiceAttribute(
        MapServiceType serviceType
    )
    {
        ServiceType = serviceType;
    }

    public MapServiceType ServiceType { get; }
}
