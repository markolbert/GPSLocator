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

        AppViewModel = (App.Current.Resources["AppConfiguration"] as AppViewModel)!;

        Logger = logger;
        Logger.SetLoggedType( GetType() );

        if( AppViewModel.IsValid )
            AppViewModel.SetStatusMessage("Ready" );
        else AppViewModel.SetStatusMessage("Invalid configuration", StatusMessageType.Important );
    }

    protected IJ4JLogger Logger { get; }
    public AppViewModel AppViewModel { get; }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        Messenger.UnregisterAll(this);
    }
}
