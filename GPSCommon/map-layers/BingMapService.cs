using System.Collections.Generic;
using System.Globalization;
using J4JSoftware.Logging;
using MapControl;

namespace J4JSoftware.GPSCommon;

public class BingMapService : MapService
{
    public BingMapService(
        IBaseAppConfig appConfig,
        IJ4JLogger logger
    )
        : base( appConfig, MapServiceType.BingMaps, logger )
    {
    }

    public string Culture { get; set; }= CultureInfo.CurrentUICulture.Name;

    public override IEnumerable<MapServiceInfo> GetMapServices()
    {
        if( string.IsNullOrEmpty( ApiKey ) )
        {
            Logger.Error( "No API key defined for Bing Maps service" );
            yield break;
        }

        BingMapsTileLayer.ApiKey = ApiKey;

        yield return new MapServiceInfo( "Bing Maps (Road)",
                                         new BingMapsTileLayer
                                         {
                                             Mode = MapControl.BingMapsTileLayer.MapMode.Road, Culture = Culture
                                         } );

        yield return new MapServiceInfo( "Bing Maps (Aerial)",
                                         new BingMapsTileLayer
                                         {
                                             Mode = MapControl.BingMapsTileLayer.MapMode.Aerial, Culture = Culture
                                         } );

        yield return new MapServiceInfo( "Bing Maps (Aerial w/Labels",
                                         new BingMapsTileLayer
                                         {
                                             Mode = MapControl.BingMapsTileLayer.MapMode.AerialWithLabels,
                                             Culture = Culture
                                         } );
    }
}
