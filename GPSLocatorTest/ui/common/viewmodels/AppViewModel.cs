using System.ComponentModel;
using System.Text.Json.Serialization;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator;

public class AppViewModel : BaseAppViewModel<AppConfig>
{
    public AppViewModel(
        AppConfig appConfig,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( appConfig, statusMessages, logger )
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