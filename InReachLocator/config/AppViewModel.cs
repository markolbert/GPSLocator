using System.Collections.Generic;
using System.Text.Json.Serialization;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace J4JSoftware.InReach
{
    public class AppViewModel : ObservableObject
    {
        public static ResourceNames ResourceNames { get; } = new();

        private readonly List<NetEventArgs> _netEvents = new();

        private bool _isValid;
        private string? _statusMesg;
        private Style? _statusStyle;
        private Visibility _determinateVisibility = Visibility.Collapsed;
        private double _progressBarMax;
        private double _progressBarValue;
        private Visibility _indeterminateVisibility = Visibility.Collapsed;

        public AppViewModel()
        {
            Configuration = App.Current.Host.Services.GetRequiredService<AppConfig>();

            var logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
            logger.LogEvent += Logger_LogEvent;
        }

        private void Logger_LogEvent(object? sender, NetEventArgs e)
        {
            LogEvents.AddLogEvent( e );
        }

        public AppConfig Configuration { get; set; }

        [JsonIgnore]
        public bool IsValid
        {
            get=> _isValid;

            set
            {
                var changed = value != _isValid;
                _isValid = value;

                if( changed )
                    OnPropertyChanged( nameof( IsValid ) );
            }
        }

        [JsonIgnore]
        public IndexedLogEvent.Collection LogEvents { get; } = new();

        public bool UseImperialUnits
        {
            get => Configuration.UseImperialUnits;

            set
            {
                Configuration.UseImperialUnits = value;
                OnPropertyChanged( nameof( UseImperialUnits ) );
            }
        }

        public bool UseCompassHeadings
        {
            get => Configuration.UseCompassHeadings;

            set
            {
                Configuration.UseCompassHeadings = value;
                OnPropertyChanged( nameof( UseCompassHeadings ) );
            }
        }

        #region Progress bar

        public void ShowDeterminateProgressBar( double max, double value = 0 )
        {
            if( max <= 0 )
                return;

            ProgressBarMaximum = max;
            ProgressBarValue = value;

            DeterminateVisibility = Visibility.Visible;
        }

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

            set
            {
                value = value > ProgressBarMaximum ? ProgressBarMaximum : value;

                SetProperty( ref _progressBarValue, value );
            }
        }

        [JsonIgnore]
        public double ProgressBarMaximum
        {
            get => _progressBarMax;
            set => SetProperty(ref _progressBarMax, value);
        }

        [ JsonIgnore ]
        public Visibility IndeterminateVisibility
        {
            get => _indeterminateVisibility;

            set
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

        public void SetStatusMessage( string mesg, StatusMessageType type = StatusMessageType.Normal )
        {
            StatusMessage = mesg;
            
            StatusMessageStyle = type switch
            {
                StatusMessageType.Important =>
                    App.Current.Resources[AppViewModel.ResourceNames.ImportantStyleKey] as Style,
                StatusMessageType.Urgent => App.Current.Resources[AppViewModel.ResourceNames.UrgentStyleKey] as Style,
                _ => App.Current.Resources[AppViewModel.ResourceNames.NormalStyleKey] as Style
            };
        }
    }
}
