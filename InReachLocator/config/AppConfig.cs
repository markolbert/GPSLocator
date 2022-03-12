using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using J4JSoftware.DependencyInjection;
using J4JSoftware.InReach.Annotations;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Serilog.Events;

namespace J4JSoftware.InReach
{
    public class AppConfig : ObservableRecipient
    {
        public static ResourceNames ResourceNames { get; } = new();

        private readonly List<NetEventArgs> _netEvents = new();

        private bool _isValid;
        private ProgressBarState? _progressBarState;
        private int? _progressBarMax;
        private int? _progressBarValue;
        private StatusMessage? _statusMesg;

        public AppConfig()
        {
            InReachConfig = App.Current.Host.Services.GetRequiredService<InReachConfig>();

            var logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
            logger.LogEvent += Logger_LogEvent;

            IsActive = true;
        }

        private void Logger_LogEvent(object? sender, NetEventArgs e)
        {
            LogEvents.AddLogEvent( e );
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<AppConfig, ProgressBarActionMessage, string>(
                this,
                ResourceNames.ProgressBarMessageToken,
                ProgressBarActionHandler);

            Messenger.Register<AppConfig, ProgressBarIncrementMessage, string>(
                this,
                ResourceNames.ProgressBarMessageToken,
                ProgressBarIncrementHandler);

            Messenger.Register<AppConfig, StatusMessage, string>(
                this,
                ResourceNames.StatusMessageToken,
                StatusBarMessageHandler);
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll(this);
        }

        private void ProgressBarActionHandler(AppConfig recipient, ProgressBarActionMessage message)
        {
            switch (message.MessageType)
            {
                case ProgressBarMessageType.Finish:
                    ProgressBarState = null;
                    ProgressBarValue = null;

                    break;

                case ProgressBarMessageType.Pause:
                    ProgressBarState = null;
                    break;

                case ProgressBarMessageType.Start:
                    ProgressBarState = message.State;

                    if (message.State is DeterminantProgressBar dState)
                        ProgressBarMaximum = dState.Maximum;
                    else ProgressBarMaximum = null;

                    break;
            }
        }

        private void ProgressBarIncrementHandler(AppConfig recipient, ProgressBarIncrementMessage message)
        {
            if (ProgressBarValue != null)
                ProgressBarValue += message.Increment;
        }

        private void StatusBarMessageHandler(AppConfig recipient, StatusMessage message)
        {
            StatusMessage = message;
        }

        public InReachConfig InReachConfig { get; set; }

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

        [JsonIgnore]
        public ProgressBarState? ProgressBarState
        {
            get => _progressBarState;
            set => SetProperty(ref _progressBarState, value);
        }

        [JsonIgnore]
        public int? ProgressBarValue
        {
            get => _progressBarValue;
            set => SetProperty(ref _progressBarValue, value);
        }

        [JsonIgnore]
        public int? ProgressBarMaximum
        {
            get => _progressBarMax;

            set
            {
                SetProperty(ref _progressBarMax, value);
                UpdateProgressLogStatusVisibility();
            }
        }

        [JsonIgnore]
        public StatusMessage? StatusMessage
        {
            get => _statusMesg;

            private set
            {
                SetProperty(ref _statusMesg, value);
                UpdateProgressLogStatusVisibility();
            }
        }

        private void UpdateProgressLogStatusVisibility()
        {
            OnPropertyChanged(nameof(ProgressBarVisibility));
            OnPropertyChanged(nameof(StatusMessageVisibility));
        }

        [JsonIgnore]
        public Visibility ProgressBarVisibility => _progressBarState != null ? Visibility.Visible : Visibility.Collapsed;

        [ JsonIgnore ]
        public Visibility StatusMessageVisibility =>
            _progressBarState == null && !string.IsNullOrEmpty( _statusMesg?.Message )
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}
