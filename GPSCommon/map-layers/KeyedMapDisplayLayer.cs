using J4JSoftware.DependencyInjection;
using MapControl;

namespace J4JSoftware.GPSCommon;

public abstract class KeyedMapDisplayLayer : IMapDisplayLayer
{
    protected KeyedMapDisplayLayer(
        string description
    )
    {
        Description = description;
    }

    public abstract string SourceName { get; }
    public string Description { get; }

    public KeyedMapType MapType { get; set; } = KeyedMapType.Undefined;
    public bool IsValid => MapType != KeyedMapType.Undefined;

    public EncryptedString ApiKey { get; } = new();

    public abstract MapTileLayer GetMapLayer();
}
