using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator
{
    public class LaunchViewModel : ObservableObject
    {
        public event EventHandler? Initialized;

        private readonly IJ4JLogger _logger;
        private readonly AppViewModel _appViewModel;
        private readonly DispatcherQueue _dQueue;

        private string? _mesg;
        private Color _mesgColor = Colors.Gold;

        public LaunchViewModel( IJ4JLogger logger )
        {
            _dQueue = DispatcherQueue.GetForCurrentThread();

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            _appViewModel = (App.Current.Resources["AppViewModel"] as AppViewModel)!;
        }

        public void OnPageActivated()
        {
            _appViewModel.Configuration.Validation += OnValidationProgress;

            Task.Run( async () => await _appViewModel.Configuration.ValidateAsync() );
        }

        private void OnValidationProgress( object? sender, ValidationPhase args )
        {
            OnValidationUpdate( args );

            if( args is not (ValidationPhase.Failed or ValidationPhase.Succeeded) )
                return;

            Thread.Sleep( 1000 ); // so last message will be visible
            Initialized?.Invoke( this, EventArgs.Empty );
        }

        private void OnValidationUpdate( ValidationPhase args )
        {
            _dQueue.TryEnqueue( () =>
            {
                var msg = args switch
                {
                    ValidationPhase.Starting => "Validation starting",
                    ValidationPhase.NotValidatable => "Validation failed due to invalid initialization",
                    ValidationPhase.CheckingCredentials => "Checking credentials",
                    ValidationPhase.CheckingImei => "Checking IMEI",
                    ValidationPhase.Succeeded => "Validation succeeded",
                    ValidationPhase.Failed => "Validation failed",
                    _ => throw new InvalidEnumArgumentException( $"Unsupported {typeof( ValidationPhase )} '{args}'" )
                };

                Message = msg;
                MessageColor = args == ValidationPhase.Failed ? Colors.Red : Colors.Gold;

                if( args is not (ValidationPhase.Succeeded or ValidationPhase.Failed) )
                    return;

                _appViewModel.Configuration.Validation -= OnValidationProgress;
            });
        }

        public string? Message
        {
            get => _mesg;
            set => SetProperty( ref _mesg, value );
        }

        public Color MessageColor
        {
            get => _mesgColor;
            set => SetProperty( ref _mesgColor, value );
        }
    }
}
