using System.ComponentModel;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.InReach;

public class BasePassiveViewModel : ObservableObject
{
    private bool _validConfig;

    protected BasePassiveViewModel(
        IAppConfig configuration,
        IJ4JLogger logger
    )
    {
        Configuration = configuration;
        Configuration.PropertyChanged += ConfigOnPropertyChanged;

        ValidConfiguration = Configuration.IsValid;

        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    private void ConfigOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if( !string.Equals( e.PropertyName, nameof( IAppConfig.IsValid ) ) )
            return;

        ValidConfiguration = Configuration.IsValid;
    }

    protected IJ4JLogger Logger { get; }
    protected IAppConfig Configuration { get; }

    public bool ValidConfiguration
    {
        get => _validConfig;
        set => SetProperty( ref _validConfig, value );
    }
}
