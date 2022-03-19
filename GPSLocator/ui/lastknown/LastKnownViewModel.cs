using System;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator
{
    public class LastKnownViewModel : LocationMapViewModel
    {
        private MapPoint? _lastKnownPoint;

        public LastKnownViewModel(
            IJ4JLogger logger
        )
        : base(logger)
        {
        }

        public async Task OnPageActivated()
        {
            await RefreshHandlerAsync();
        }

        protected override async Task RefreshHandlerAsync()
        {
            if( !AppViewModel.Configuration.IsValid )
            {
                AppViewModel.SetStatusMessage( "Invalid configuration", StatusMessageType.Urgent );
                return;
            }

            var request = new LastKnownLocationRequest<Location>( AppViewModel.Configuration, Logger );

            DeviceResponse<LastKnownLocation<Location>>? response = null;
            await Task.Run( async () =>
            {
                response = await ExecuteRequestAsync( request, OnRequestStarted, OnRequestEnded );
            } );

            if ( !response!.Succeeded || response.Result!.Locations.Count == 0 )
            {
                await AppViewModel.SetStatusMessagesAsync( 2000,
                                                           new StatusMessage(
                                                               "Couldn't retrieve last known location",
                                                               StatusMessageType.Important ),
                                                           new StatusMessage( "Ready" ) );

                if ( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );
            }
            else
            {
                ClearMappedPoints();

                var lastLoc = response.Result.Locations[0];
                lastLoc.CompassHeadings = AppViewModel.Configuration.UseCompassHeadings;
                lastLoc.ImperialUnits = AppViewModel.Configuration.UseImperialUnits;

                var mapPoint = AddLocation(lastLoc);
                mapPoint.DisplayOnMap = MapPointDisplay.Fixed;

                LastKnownPoint = MappedPoints[0];

                await AppViewModel.SetStatusMessagesAsync(1000,
                                                          new StatusMessage(
                                                              "Retrieved last known location",
                                                              StatusMessageType.Important),
                                                          new StatusMessage("Ready"));
            }

        }

        private void OnRequestStarted()
        {
            AppViewModel.SetStatusMessage( "Updating last known location" );
            AppViewModel.IndeterminateVisibility = Visibility.Visible;
            RefreshEnabled = false;
        }

        private void OnRequestEnded()
        {
                AppViewModel.IndeterminateVisibility = Visibility.Collapsed;
                RefreshEnabled = true;
        }

        public MapPoint? LastKnownPoint
        {
            get => _lastKnownPoint;
            set => SetProperty( ref _lastKnownPoint, value );
        }
    }
}
