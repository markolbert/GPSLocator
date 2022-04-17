using J4JSoftware.GPSCommon;

namespace J4JSoftware.GPSLocator;

public interface IAppConfig : IBaseAppConfig
{
    bool UseImperialUnits { get; set; }
    bool UseCompassHeadings { get; set; }
    string? DefaultCallback { get; set; }
    int DefaultDaysBack { get; set; }
    int MaxSmsLength { get; }
}
