using System;
using J4JSoftware.Logging;
using MapControl;

namespace J4JSoftware.GPSCommon;

public class OpenMapSingleLayerGenerator : MapLayerGenerator
{
    public OpenMapSingleLayerGenerator(
        MapType mapType,
        string tileUri,
        string label,
        string? copyrightText,
        Uri? copyrightUri,
        IJ4JLogger logger
    )
        : base( mapType, label, copyrightText, copyrightUri, null, logger )
    {
        TileUri = tileUri;
    }

    public string TileUri { get; }

    public override MapTileLayer? GetMapTileLayer()
    {
        if( IsValid )
            return new MapTileLayer()
            {
                TileSource = new TileSource() { UriFormat = TileUri },
                SourceName = Label,
                Description = CopyrightText
            };

        Logger.Error( "Trying to retrieve MapTileLayer from invalid generator '{0}'", GetType() );

        return null;
    }
}
