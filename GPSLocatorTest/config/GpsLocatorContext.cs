using J4JSoftware.DependencyInjection;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public record GpsLocatorContext : IGpsLocatorContext
{
    public GpsLocatorContext(
        IJ4JProtection protector,
        IJ4JLogger? logger,
        IBullshitLogger? bsLogger
    )
    {
        Protector = protector;

        Logger = logger;
        Logger?.SetLoggedType(GetType());

        BSLogger = bsLogger;
    }

    public IJ4JLogger? Logger { get; }
    public IBullshitLogger? BSLogger { get; }
    public IJ4JProtection Protector { get; init; }

    public int MaxSmsLength => 160;
    public string HelpLink => "https://www.jumpforjoysoftware.com/gpslocator-user-docs/";
}
