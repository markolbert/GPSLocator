using System.Collections.Generic;

namespace J4JSoftware.GPSCommon;

public interface IMapService
{
    MapServiceType ServiceType { get; }
    IEnumerable<MapServiceInfo> GetMapServices();
}
