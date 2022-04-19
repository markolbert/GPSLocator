using System.ComponentModel;

namespace J4JSoftware.GPSCommon;

public enum MapType
{
    [Description("Bing (Roads)")]
    [MapService(MapServiceType.BingMaps)]
    BingRoads,

    [Description("Bing (Aerial)")]
    [MapService(MapServiceType.BingMaps)]
    BingAerial,

    [Description("Bing (Aerial w/Labels)")]
    [MapService(MapServiceType.BingMaps)]
    BingAerialWithLabels,
    
    OpenStreetMap,
    OpenTopoMap
}