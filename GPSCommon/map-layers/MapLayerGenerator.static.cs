using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using MapControl;

namespace J4JSoftware.GPSCommon;

public abstract partial class MapLayerGenerator
{
    public static List<MapLayerGenerator> GetCollection(
        List<MapServiceCredentials> credentials,
        MapBase map,
        IJ4JLogger logger
    )
    {
        var retVal = new List<MapLayerGenerator>();

        retVal.Add( new OpenMapSingleLayerGenerator( MapType.OpenStreetMap,
                                                     "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                                                     "OpenStreetMap",
                                                     "© OpenStreetMap Contributors",
                                                     new Uri( "http://www.openstreetmap.org/copyright" ),
                                                     logger ) );

        retVal.Add( new OpenMapSingleLayerGenerator( MapType.OpenTopoMap,
                                                     "https://tile.opentopomap.org/{z}/{x}/{y}.png",
                                                     "OpenTopoMap",
                                                     "© OpenTopoMap-Mitwirkende, SRTM | Kartendarstellung\n© OpenTopoMap\nCC-BY-SA",
                                                     new Uri( "http://opentopomap.org/" ),
                                                     logger ) );

        var bingCredentials = credentials.FirstOrDefault( x => x.ServiceType == MapServiceType.BingMaps )?.ApiKey;
        if( bingCredentials != null )
            AddBingMapTypes( bingCredentials, retVal, logger );

        var googleApiKey = "AIzaSyDSGlR6-fVszRgSQUZ1EN59ft5UfJsRt2Q";

        AddGoogleMapTypes( googleApiKey, map, retVal, logger );

        return retVal;
    }

    private static void AddBingMapTypes( string apiKey, List<MapLayerGenerator> generators, IJ4JLogger logger )
    {
        var bingCopyright = new Uri( "https://www.microsoft.com/en-us/maps/product/enduserterms" );

        generators.Add( new BingMapLayerGenerator( MapType.BingAerial,
                                                   apiKey,
                                                   "© Microsoft Corporation",
                                                   bingCopyright,
                                                   logger ) );

        generators.Add( new BingMapLayerGenerator( MapType.BingAerialWithLabels,
                                                   apiKey,
                                                   "© Microsoft Corporation",
                                                   bingCopyright,
                                                   logger ) );

        generators.Add( new BingMapLayerGenerator( MapType.BingRoads,
                                                   apiKey,
                                                   "© Microsoft Corporation",
                                                   bingCopyright,
                                                   logger ) );
    }

    private static void AddGoogleMapTypes( string apiKey, MapBase map, List<MapLayerGenerator> generators, IJ4JLogger logger )
    {
        generators.Add( new GoogleMapLayerGenerator( map,
                                                     MapType.GoogleMaps,
                                                     apiKey,
                                                     "© Google",
                                                     new Uri( "https://www.google.com" ),
                                                     logger ) );
    }
}
