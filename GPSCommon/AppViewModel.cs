﻿using System.ComponentModel;
using System.Text.Json.Serialization;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSCommon;

public class AppViewModel : ObservableObject
{
    private string? _statusMesg;
    private Style? _statusStyle;
    private Visibility _determinateVisibility = Visibility.Collapsed;
    private double _progressBarMax;
    private double _progressBarValue;
    private Visibility _indeterminateVisibility = Visibility.Collapsed;

    public AppViewModel()
    {
        Configuration = App.Current.Host.Services.GetRequiredService<AppConfig>();
        Configuration.PropertyChanged += ConfigurationOnPropertyChanged;

        var logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
        logger.LogEvent += Logger_LogEvent;

        var statusMessages = App.Current.Host.Services.GetRequiredService<StatusMessage.StatusMessages>();
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

    public AppConfig Configuration { get; }

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

        StatusMessageStyle = args.Importance switch
        {
            MessageLevel.Important =>
                App.Current.Resources[ResourceNames.ImportantStyleKey] as Style,
            MessageLevel.Urgent =>
                App.Current.Resources[ResourceNames.UrgentStyleKey] as Style,
            _ => App.Current.Resources[ResourceNames.NormalStyleKey] as Style
        };

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
}