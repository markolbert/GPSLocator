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

        public async Task OnPageActivated()
        {
            var isValid = await _appViewModel.Configuration.ValidateAsync( StatusChanged );

            WeakReferenceMessenger.Default.Send( new AppConfiguredMessage( isValid ), "primary" );
        }

        private void StatusChanged( object? sender, DeviceRequestEventArgs args)
        {
            var error = args.Message ?? "Unspecified error";

            _dQueue.TryEnqueue(() =>
            {
                Message = args.RequestEvent switch
                {
                    RequestEvent.Started => "Validating configuration",
                    RequestEvent.Succeeded => "Configuration is valid",
                    RequestEvent.Aborted => $"Configuration failed: {error}",
                    _ => throw new InvalidOperationException( $"Unsupported RequestEvent '{args.RequestEvent}'" )
                };
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
