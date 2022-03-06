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
    public class LastKnownViewModel : LocationMapViewModel
    {
        private MapLocation? _lastLocation;

        public LastKnownViewModel(
            IJ4JLogger logger
        )
        : base(logger)
        {
            IsActive = true;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<LastKnownViewModel, MapLocation, string>(this, "primary", NewLocationHandler);
        }

        private void NewLocationHandler( LastKnownViewModel recipient, MapLocation mapLocation )
        {
            //LocationViewModel = new LocationViewModel( mapLocation, Logger );
            mapLocation.LocationType = LocationType.Pushpin;

            ClearMapLocations();
            AddMapLocation( mapLocation );

            LastLocation = mapLocation;

            //LocationUrl = $"https://maps.google.com?q={LocationViewModel.Latitude},{LocationViewModel.Longitude}";
        }

        public MapLocation? LastLocation
        {
            get => _lastLocation;
            set => SetProperty( ref _lastLocation, value );
        }
    }
}
