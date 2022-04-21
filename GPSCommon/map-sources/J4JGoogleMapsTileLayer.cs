// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Common.Enums;
using GoogleApiCommon = GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.StaticMaps.Request;
using GoogleApi.Entities.Maps.StaticMaps.Response;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.UI.Xaml;
#pragma warning disable CS8618

namespace J4JSoftware.GPSCommon;

public class J4JGoogleMapsTileLayer : MapTileLayer
{
    private readonly string _apiKey;
    private readonly IJ4JLogger _logger;

    public J4JGoogleMapsTileLayer(
        string apiKey,
        MapBase map,
        IJ4JLogger logger
    )
        : base( new GoogleTileImageLoader( map, apiKey, logger ) )
    {
        _apiKey = apiKey;

        _logger = logger;
        _logger.SetLoggedType( GetType() );

        MinZoomLevel = 1;
        MaxZoomLevel = 21;

        Loaded += OnLoaded;
    }

    private async void OnLoaded( object sender, RoutedEventArgs args )
    {
        Loaded -= OnLoaded;

        if( string.IsNullOrEmpty( _apiKey ) )
        {
            _logger.Error( "J4JGoogleMapsTileLayer requires a Google Maps API Key" );
            return;
        }

        var mapReq = new StaticMapsRequest()
        {
            Center = new GoogleApiCommon.Location( new Address( "San Francisco, CA" ) ),
            Key = _apiKey,
            ZoomLevel = 9
        };

        StaticMapsResponse? mapResp = null;

        try
        {
            mapResp = await GoogleApi.GoogleMaps.StaticMaps.QueryAsync( mapReq );
        }
        catch( Exception ex )
        {
            _logger.Error<string>("Failed to validate access to Google Maps API, message was '{0}'", ex.Message);
            return;
        }

        if( mapResp == null || mapResp.Status != Status.Ok )
        {
            _logger.Error(mapResp?.ErrorMessage ?? "Google Maps query returned null response");
            return;
        }

        TileSource = new GoogleMapsTileSource( _logger );
    }
}