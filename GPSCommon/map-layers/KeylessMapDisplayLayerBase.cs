using MapControl;

namespace J4JSoftware.GPSCommon;

public record KeylessMapDisplayLayerBase : IKeylessMapDisplayLayer
{
    protected KeylessMapDisplayLayerBase(
        string uriFormat,
        string srcName,
        string description
    )
    {
        UriFormat = uriFormat;
        SourceName = srcName;
        Description = description;
    }

    public string UriFormat { get; init; }
    public string SourceName { get; init; }
    public string Description { get; init; }

    public virtual MapTileLayer GetMapLayer() =>
        new()
        {
            TileSource = new TileSource() { UriFormat = UriFormat },
            SourceName = SourceName,
            Description = Description
        };
}
