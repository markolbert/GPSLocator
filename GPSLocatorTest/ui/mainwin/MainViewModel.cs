using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public class MainViewModel : BaseViewModel<AppConfig>
{
    public MainViewModel(
        AppViewModel appViewModel,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger,
        IBullshitLogger bsLogger
    )
        : base( appViewModel, statusMessages, logger, bsLogger )
    {
        IsActive = true;
    }
}