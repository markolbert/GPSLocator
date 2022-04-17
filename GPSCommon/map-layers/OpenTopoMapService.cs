using System.Collections.Generic;
using J4JSoftware.Logging;
using MapControl;

namespace J4JSoftware.GPSCommon;

public class OpenTopoMapService : MapService
{
    public const string UriFormat = "https://tile.opentopomap.org/{z}/{x}/{y}.png";

    public OpenTopoMapService(
        IBaseAppConfig appConfig,
        IJ4JLogger logger
    )
        : base( appConfig,
                MapServiceType.OpenTopoMap,
                logger )
    {
    }

    public override IEnumerable<MapServiceInfo> GetMapServices()
    {
        yield return new MapServiceInfo( "OpenTopoMap",
                                         new MapTileLayer()
                                         {
                                             TileSource = new TileSource() { UriFormat = UriFormat },
                                             SourceName = "OpenTopMap",
                                             Description = "© [OpenTopoMap](https://openstreetmap.org/copyright)-Mitwirkende, SRTM | Kartendarstellung: © [OpenTopoMap](http://opentopomap.org/) ([CC-BY-SA](https://creativecommons.org/licenses/by-sa/3.0/))"
                                         } );
    }
}
