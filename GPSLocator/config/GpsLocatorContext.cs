using System.Collections.Generic;
using System.Linq;
using J4JSoftware.DependencyInjection;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public record GpsLocatorContext : IGpsLocatorContext
{
    public GpsLocatorContext(
        IEnumerable<IMapDisplayLayer> mapLayers,
        IJ4JProtection protector,
        IJ4JLogger? logger
    )
    {
        MapLayers = mapLayers.ToList();
        Protector = protector;

        Logger = logger;
        Logger?.SetLoggedType(GetType());
    }

    public IJ4JLogger? Logger { get; }
    public IJ4JProtection Protector { get; init; }

    public int MaxSmsLength => 160;
    public string HelpLink => "https://www.jumpforjoysoftware.com/gpslocator-user-docs/";
    public List<IMapDisplayLayer> MapLayers { get; init; }
}
