using System.ComponentModel;
using System.Reflection;

namespace J4JSoftware.GPSCommon;

public static class MapEnumExtensions
{
    public static MapServiceType? GetMapServiceType( this MapType mapType )
    {
        var enumMember = typeof(MapType).GetMember(mapType.ToString())[0];
        var svcAttr = enumMember.GetCustomAttribute<MapServiceAttribute>(false);

        return svcAttr?.ServiceType;
    }

    public static string GetDescription( this MapType mapType )
    {
        var enumMember = typeof(MapType).GetMember(mapType.ToString())[0];
        var svcAttr = enumMember.GetCustomAttribute<DescriptionAttribute>(false);

        return svcAttr?.Description ?? mapType.ToString();
    }
}
