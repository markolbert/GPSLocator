using System.Collections.Generic;
using J4JSoftware.Logging;
using MapControl;

namespace J4JSoftware.GPSCommon;

public class OpenStreetMapService : MapService
{
    public const string UriFormat = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
    public const string Name = "OpenStreetMap";

    public OpenStreetMapService(
        IBaseAppConfig appConfig,
        IJ4JLogger logger
    )
        : base( appConfig,
                MapServiceType.OpenStreetMap,
                logger )
    {
    }

    public override IEnumerable<MapServiceInfo> GetMapServices()
    {
        yield return new MapServiceInfo( "OpenStreetMap",
                                         new MapTileLayer()
                                         {
                                             TileSource = new TileSource() { UriFormat = UriFormat },
                                             SourceName = Name,
                                             Description = "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)"
                                         } );
    }
}
