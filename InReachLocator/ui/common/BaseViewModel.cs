using System.Collections.ObjectModel;
using System.ComponentModel;
using J4JSoftware.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.InReach;

public class BaseViewModel : ObservableRecipient
{
    protected BaseViewModel(
        IJ4JLogger logger
    )
    {
        IsActive = true;

        Configuration = (App.Current.Resources["AppConfiguration"] as AppConfigViewModel)!;

        Logger = logger;
        Logger.SetLoggedType( GetType() );

        if( Configuration.IsValid )
            StatusMessage.Send( "Ready" );
        else StatusMessage.Send( "Invalid configuration", StatusMessageType.Important );
    }

    protected IJ4JLogger Logger { get; }
    public AppConfigViewModel Configuration { get; }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        Messenger.UnregisterAll(this);
    }
}
