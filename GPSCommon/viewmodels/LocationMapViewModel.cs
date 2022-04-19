using J4JSoftware.Logging;

namespace J4JSoftware.GPSCommon;

public class LocationMapViewModel<TAppConfig> : BaseViewModel
    where TAppConfig : BaseAppConfig
{
    protected LocationMapViewModel(
        MapViewModel retrievedPts,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( statusMessages, logger )
    {
        MapViewModel = retrievedPts;
    }

    public MapViewModel MapViewModel { get; }
}