using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public interface IDeviceContext
{
    IJ4JProtection Protector { get; }
    IJ4JLogger? Logger { get; }
    IBullshitLogger? BSLogger { get; }
}
