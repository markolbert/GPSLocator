using System;
using System.ComponentModel;
using J4JSoftware.Logging;
using MapControl;

namespace J4JSoftware.GPSCommon;

public class BingMapLayerGenerator : MapLayerGenerator
{
    private readonly string _apiKey;

    public BingMapLayerGenerator(
        MapType mapType,
        string apiKey,
        string? copyrightText,
        Uri? copyrightUri,
        IJ4JLogger logger,
        string? culture = null
    )
        : base( mapType, mapType.GetDescription(), copyrightText, copyrightUri, MapServiceType.BingMaps, logger )
    {
        _apiKey = apiKey;

        if( string.IsNullOrEmpty( _apiKey ) )
        {
            Logger.Error( "API key is empty or undefined" );
            IsValid = false;
        }

        Culture = culture;
    }

    public string? Culture { get; }

    public override MapTileLayer? GetMapTileLayer()
    {
        if( IsValid )
        {
            var bingType = MapType switch
            {
                MapType.BingAerial => J4JBingMapsTileLayer.MapMode.Aerial,
                MapType.BingAerialWithLabels => J4JBingMapsTileLayer.MapMode.AerialWithLabels,
                MapType.BingRoads => J4JBingMapsTileLayer.MapMode.Road,
                _ => throw new InvalidEnumArgumentException( $"Unsupported {typeof( MapType )} '{MapType}'" )
            };

            return new J4JBingMapsTileLayer(_apiKey, Logger) { Mode = bingType, Culture = Culture };
        }

        Logger.Error( "Trying to retrieve MapTileLayer from invalid generator '{0}'", GetType() );

        return null;
    }
}
