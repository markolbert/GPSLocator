using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.GPSLocator
{
    public class AppViewModel : ObservableObject
    {
        public static ResourceNames ResourceNames { get; } = new();

        public static List<SingleSelectableItem> PageNames { get; } = new()
        {
            new SingleSelectableItem( null, ResourceNames.NullPageName ),
            new SingleSelectableItem( typeof( LastKnownPage ),
                                      ResourceNames.LastKnownPageName,
                                      "Last Known Location" ),
            new SingleSelectableItem( typeof( HistoryPage ), ResourceNames.HistoryPageName, "History" ),
            new SingleSelectableItem( typeof( MessagingPage ), ResourceNames.MessagingPageName, "Messaging" ),
            new SingleSelectableItem( typeof( LogViewerPage ), ResourceNames.LogViewerPageName, "Log Viewer" ),
            new SingleSelectableItem(typeof(SettingsPage), ResourceNames.SettingsPageName, "Settings")
        };

        private readonly List<NetEventArgs> _netEvents = new();

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
        }

        private void ConfigurationOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
        {
            OnPropertyChanged(nameof(Configuration));
        }

        private void Logger_LogEvent(object? sender, NetEventArgs e)
        {
            LogEvents.AddLogEvent( e );
        }

        public AppConfig Configuration { get; set; }

        [JsonIgnore]
        public IndexedLogEvent.Collection LogEvents { get; } = new();

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

        public void SetStatusMessage( string mesg, StatusMessageType type = StatusMessageType.Normal ) =>
            SetStatusMessage( new StatusMessage( mesg, type ) );

        public async Task SetStatusMessagesAsync( int msPause, IEnumerable<StatusMessage> messages ) =>
            await SetStatusMessagesAsync( msPause, messages.ToArray() );

        public async Task SetStatusMessagesAsync( int msPause, params StatusMessage[] messages )
        {
            foreach( StatusMessage message in messages )
            {
                SetStatusMessage( message );
                await Task.Delay( msPause );
            }
        }

        public void SetStatusMessage( StatusMessage mesg )
        {
            StatusMessage = mesg.Text;

            StatusMessageStyle = mesg.Type switch
            {
                StatusMessageType.Important =>
                    App.Current.Resources[AppViewModel.ResourceNames.ImportantStyleKey] as Style,
                StatusMessageType.Urgent => App.Current.Resources[AppViewModel.ResourceNames.UrgentStyleKey] as Style,
                _ => App.Current.Resources[AppViewModel.ResourceNames.NormalStyleKey] as Style
            };
        }
    }
}
