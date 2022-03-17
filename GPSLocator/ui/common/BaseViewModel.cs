using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GPSLocator;

public class BaseViewModel : ObservableRecipient
{
    protected BaseViewModel(
        IJ4JLogger logger
    )
    {
        IsActive = true;

        AppViewModel = (App.Current.Resources["AppViewModel"] as AppViewModel)!;

        Logger = logger;
        Logger.SetLoggedType( GetType() );

        if( AppViewModel.Configuration.IsValid )
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
