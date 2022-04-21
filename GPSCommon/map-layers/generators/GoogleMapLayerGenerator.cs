using System;
using System.ComponentModel;
using J4JSoftware.Logging;
using MapControl;

namespace J4JSoftware.GPSCommon;

public class GoogleMapLayerGenerator : MapLayerGenerator
{
    private readonly string _apiKey;
    private readonly MapBase _map;

    public GoogleMapLayerGenerator(
        MapBase map,
        MapType mapType,
        string apiKey,
        string? copyrightText,
        Uri? copyrightUri,
        IJ4JLogger logger
    )
        : base( mapType, mapType.GetDescription(), copyrightText, copyrightUri, MapServiceType.GoogleMaps, logger )
    {
        _map = map;
        _apiKey = apiKey;

        if( string.IsNullOrEmpty( _apiKey ) )
        {
            Logger.Error( "API key is empty or undefined" );
            IsValid = false;
        }
    }

    public override MapTileLayer? GetMapTileLayer()
    {
        if( IsValid )
            return new J4JGoogleMapsTileLayer(_apiKey, _map, Logger);

        Logger.Error( "Trying to retrieve MapTileLayer from invalid generator '{0}'", GetType() );

        return null;
    }
}
