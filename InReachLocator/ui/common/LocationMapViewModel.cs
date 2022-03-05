using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace J4JSoftware.InReach
{
    public class LocationMapViewModel : ObservableRecipient
    {
        private LocationViewModel? _locationViewModel;
        private string? _locationUrl;
        private double _mapHeight = 500;
        private double _mapWidth = 500;

        protected LocationMapViewModel(
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<LocationMapViewModel, SizeMessage, string>(this, "mainwindow", WindowSizeChangedHandler);
        }

        private void WindowSizeChangedHandler( LocationMapViewModel recipient, SizeMessage message )
        {
            MapHeight = message.Height - 50;
            MapWidth = message.Width - GridWidth - 10;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll( this );
        }

        public LocationViewModel? LocationViewModel
        {
            get => _locationViewModel;
            protected set => SetProperty( ref _locationViewModel, value );
        }

        public string? LocationUrl
        {
            get => _locationUrl;
            set => SetProperty( ref _locationUrl, value );
        }

        public double GridHeight { get; set; }
        public double GridWidth { get; set; }

        public double MapHeight
        {
            get => _mapHeight;

            set
            {
                if( value < 50 )
                    return;

                SetProperty( ref _mapHeight, value );
            }
        }

        public double MapWidth
        {
            get => _mapWidth;

            set
            {
                if( value < 50 )
                    return;

                SetProperty( ref _mapWidth, value );
            }
        }
    }
}
