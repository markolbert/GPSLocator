using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public class MainViewModel : BaseViewModel
{
    public MainViewModel(
        AppViewModel appViewModel,
        IJ4JLogger logger 
    )
        : base( appViewModel, logger )
    {
        IsActive = true;
    }
}