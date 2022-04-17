using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public class MainViewModel : BaseViewModel
{
    public MainViewModel(
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( statusMessages, logger )
    {
        IsActive = true;
    }
}