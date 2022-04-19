// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.UI.Xaml;
#pragma warning disable CS8618

namespace J4JSoftware.GPSCommon;

/// <summary>
/// Displays Bing Maps tiles. 
/// Tile image URLs and min/max zoom levels are retrieved from the Imagery Metadata Service
/// (see http://msdn.microsoft.com/en-us/library/ff701716.aspx).
/// </summary>
public class J4JBingMapsTileLayer : MapTileLayer
{
    public enum MapMode
    {
        Road, Aerial, AerialWithLabels
    }

    private readonly string _apiKey;
    private readonly IJ4JLogger _logger;

    public J4JBingMapsTileLayer( 
        string apiKey,
        IJ4JLogger logger
        )
        : this( apiKey, logger, new TileImageLoader())
    {
    }

    public J4JBingMapsTileLayer(
        string apiKey, 
        IJ4JLogger logger,
        ITileImageLoader tileImageLoader 
        )
        : base(tileImageLoader)
    {
        _apiKey = apiKey;
        
        _logger = logger;
        _logger.SetLoggedType( GetType() );

        MinZoomLevel = 1;
        MaxZoomLevel = 21;
        Loaded += OnLoaded;
    }

    public MapMode Mode { get; set; }
    public string? Culture { get; set; }
    public Uri LogoImageUri { get; private set; }

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        Loaded -= OnLoaded;

        if (!string.IsNullOrEmpty(_apiKey))
        {
            var metadataUri = $"http://dev.virtualearth.net/REST/V1/Imagery/Metadata/{Mode}?output=xml&key={_apiKey}";

            try
            {
                await using var stream = await ImageLoader.HttpClient.GetStreamAsync(metadataUri);
                ReadImageryMetadata(XDocument.Load(stream).Root!);
            }
            catch (Exception ex)
            {
                _logger.Error<string, string>( "Problem retrieving Bing metadata from {0}, message was '{1}'",
                                               metadataUri,
                                               ex.Message );
            }
        }
        else
            _logger.Error("J4JBingMapsTileLayer requires a Bing Maps API Key");
    }

    private void ReadImageryMetadata(XElement metadataResponse)
    {
        var ns = metadataResponse.Name.Namespace;
        var metadata = metadataResponse.Descendants(ns + "ImageryMetadata").FirstOrDefault();

        if( metadata==null)
            _logger.Error("Failed to get ImageryMetadata");
        else
        {
            var imageUrl = metadata.Element(ns + "ImageUrl")?.Value;
            var subdomains = metadata.Element(ns + "ImageUrlSubdomains")?.Elements(ns + "string").Select(e => e.Value).ToArray();

            if (!string.IsNullOrEmpty(imageUrl) && subdomains != null && subdomains.Length > 0)
            {
                var zoomMin = metadata.Element(ns + "ZoomMin")?.Value;
                var zoomMax = metadata.Element(ns + "ZoomMax")?.Value;

                if (zoomMin != null && int.TryParse(zoomMin, out int zoomLevel) && MinZoomLevel < zoomLevel)
                    MinZoomLevel = zoomLevel;

                if (zoomMax != null && int.TryParse(zoomMax, out zoomLevel) && MaxZoomLevel > zoomLevel)
                    MaxZoomLevel = zoomLevel;

                if (string.IsNullOrEmpty(Culture))
                    Culture = CultureInfo.CurrentUICulture.Name;

                TileSource = new BingMapsTileSource
                {
                    UriFormat = imageUrl.Replace("{culture}", Culture),
                    Subdomains = subdomains
                };
            }
        }

        var logoUri = metadataResponse.Element(ns + "BrandLogoUri");

        if (logoUri == null)
            _logger.Error("Failed to retrieve BrandLogoUri");
        else
        {
            LogoImageUri = new Uri(logoUri.Value);
        }
    }
}