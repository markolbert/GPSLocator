using J4JSoftware.DependencyInjection;
using J4JSoftware.GPSLocator;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSCommon;

public interface IGpsLocatorContext : ICommonAppContext
{
    int MaxSmsLength { get; }
}
