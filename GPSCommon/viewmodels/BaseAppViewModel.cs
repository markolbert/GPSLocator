using System.ComponentModel;
using System.Text.Json.Serialization;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSCommon;

public abstract class BaseAppViewModel<TAppConfig> : ObservableObject
    where TAppConfig : BaseAppConfig
{
    private string? _statusMesg;
    private Style? _statusStyle;
    private Visibility _determinateVisibility = Visibility.Collapsed;
    private double _progressBarMax;
    private double _progressBarValue;
    private Visibility _indeterminateVisibility = Visibility.Collapsed;

    protected BaseAppViewModel(
        TAppConfig appConfig,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
        )
    {
        Configuration = appConfig;
        Configuration.PropertyChanged += ConfigurationOnPropertyChanged;

        logger.LogEvent += Logger_LogEvent;
        statusMessages.DisplayMessage += OnDisplayMessage;
    }

    private void ConfigurationOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        OnPropertyChanged(nameof(Configuration));
    }

    private void Logger_LogEvent(object? sender, NetEventArgs e)
    {
        LogEvents.AddLogEvent( e );
    }

    public TAppConfig Configuration { get; }

    [JsonIgnore]
    public IndexedLogEvent.Collection LogEvents { get; } = new();

    #region Progress bar

    [JsonIgnore]
    public Visibility DeterminateVisibility
    {
        get => _determinateVisibility;

        private set
        {
            if( value == Visibility.Visible )
                IndeterminateVisibility = Visibility.Collapsed;

            SetProperty( ref _determinateVisibility, value );
        }
    }

    [JsonIgnore]
    public double ProgressBarValue
    {
        get => _progressBarValue;

        private set
        {
            value = value > ProgressBarMaximum ? ProgressBarMaximum : value;

            SetProperty( ref _progressBarValue, value );
        }
    }

    [JsonIgnore]
    public double ProgressBarMaximum
    {
        get => _progressBarMax;
        private set => SetProperty(ref _progressBarMax, value);
    }

    [ JsonIgnore ]
    public Visibility IndeterminateVisibility
    {
        get => _indeterminateVisibility;

        private set
        {
            if( value == Visibility.Visible)
                DeterminateVisibility = Visibility.Collapsed;

            SetProperty( ref _indeterminateVisibility, value );
        }
    }

    #endregion

    [JsonIgnore]
    public string? StatusMessage
    {
        get => _statusMesg;
        private set => SetProperty(ref _statusMesg, value);
    }

    [ JsonIgnore ]
    public Style? StatusMessageStyle
    {
        get => _statusStyle;
        private set => SetProperty( ref _statusStyle, value );
    }

    private void OnDisplayMessage(object? sender, StatusMessage args )
    {
        StatusMessage = args.Text;
        StatusMessageStyle = GetStatusMessageStyle( args );

        if( !args.HasProgressBar )
        {
            IndeterminateVisibility = Visibility.Collapsed;
            DeterminateVisibility = Visibility.Collapsed;

            return;
        }

        switch( args.ProgressBarType! )
        {
            case ProgressBarType.Determinate:
                ProgressBarMaximum = args.MaxProgressBar;
                ProgressBarValue = 0;

                DeterminateVisibility = Visibility.Visible;

                break;

            case ProgressBarType.Indeterminate:
                IndeterminateVisibility = Visibility.Visible;
                break;

            default:
                throw new InvalidEnumArgumentException(
                    $"Unsupported {typeof( ProgressBarType )} value '{args.ProgressBarType}'" );
        }
    }

    protected abstract Style? GetStatusMessageStyle( StatusMessage msg );
}