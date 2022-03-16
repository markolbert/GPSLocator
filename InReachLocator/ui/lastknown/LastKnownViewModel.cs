using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.InReach
{
    public class LastKnownViewModel : LocationMapViewModel
    {
        private readonly DispatcherQueue _dQueue;

        private MapPoint? _lastKnownPoint;

        public LastKnownViewModel(
            IJ4JLogger logger
        )
        : base(logger)
        {
            _dQueue = DispatcherQueue.GetForCurrentThread();
        }

        public async Task OnPageActivated()
        {
            await RefreshHandlerAsync();
        }

        protected override async Task RefreshHandlerAsync()
        {
            if( !AppViewModel.IsValid )
            {
                AppViewModel.SetStatusMessage( "Invalid configuration", StatusMessageType.Urgent );
                return;
            }

            var request = new LastKnownLocationRequest<Location>( AppViewModel.Configuration, Logger );
            request.Started += RequestStarted;
            request.Ended += RequestEnded;

            InReachResponse<LastKnownLocation<Location>>? response = null;
            await Task.Run( async () =>
            {
                response = await request.ExecuteAsync();
            } );

            if ( !response!.Succeeded || response.Result!.Locations.Count == 0 )
            {
                AppViewModel.SetStatusMessage("Couldn't retrieve last known location", StatusMessageType.Important );

                if ( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );

                return;
            }

            ClearMappedPoints();

            var lastLoc = response.Result.Locations[0];
            lastLoc.CompassHeadings = AppViewModel.UseCompassHeadings;
            lastLoc.ImperialUnits = AppViewModel.UseImperialUnits;

            var mapPoint = AddLocation( lastLoc );
            mapPoint.DisplayOnMap = MapPointDisplay.Fixed;

            LastKnownPoint = MappedPoints[ 0 ];

            AppViewModel.SetStatusMessage("Ready" );
        }

        private void RequestStarted( object? sender, EventArgs e )
        {
            _dQueue.TryEnqueue( () =>
            {
                AppViewModel.SetStatusMessage( "Updating last known location" );
                AppViewModel.IndeterminateVisibility = Visibility.Visible;
                RefreshEnabled = false;
            });
        }

        private void RequestEnded(object? sender, EventArgs e)
        {
            _dQueue.TryEnqueue( () =>
            {
                AppViewModel.IndeterminateVisibility = Visibility.Collapsed;
                RefreshEnabled = true;
            } );
        }

        public MapPoint? LastKnownPoint
        {
            get => _lastKnownPoint;
            set => SetProperty( ref _lastKnownPoint, value );
        }
    }
}
