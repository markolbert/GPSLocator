using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator;

public class AppViewModel : BaseAppViewModel
{
    public AppViewModel(
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( statusMessages, logger )
    {
    }

    protected override Style? GetStatusMessageStyle( StatusMessage msg )
    {
        return msg.Importance switch
        {
            MessageLevel.Important =>
                App.Current.Resources[ResourceNames.ImportantStyleKey] as Style,
            MessageLevel.Urgent =>
                App.Current.Resources[ResourceNames.UrgentStyleKey] as Style,
            _ => App.Current.Resources[ResourceNames.NormalStyleKey] as Style
        };
    }
}