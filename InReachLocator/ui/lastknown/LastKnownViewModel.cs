using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;

namespace J4JSoftware.InReach
{
    public class LastKnownViewModel : LocationMapViewModel
    {
        public LastKnownViewModel(
            IJ4JLogger logger
        )
        : base(logger)
        {
            //var dQueue = DispatcherQueue.GetForCurrentThread();
            //dQueue.TryEnqueue( async () =>
            //{
            //    await RefreshHandlerAsync();
            //} );

            RefreshCommand = new AsyncRelayCommand( RefreshHandlerAsync );
        }

        public async Task OnPageActivated()
        {
            await RefreshHandlerAsync();
        }

        public AsyncRelayCommand RefreshCommand { get; }

        private async Task RefreshHandlerAsync()
        {
            if( !Configuration.IsValid )
                return;

            var request = new LastKnownLocationRequest<Location>( Configuration.Configuration, Logger );
            var response = await request.ExecuteAsync();

            if( !response.Succeeded || response.Result!.Locations.Count == 0 )
            {
                if( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );

                return;
            }

            ClearMapLocations();

            var lastLoc = response.Result.Locations[0];
            lastLoc.CompassHeadings = Configuration.UseCompassHeadings;
            lastLoc.ImperialUnits = Configuration.UseImperialUnits;

            AddPushpin( lastLoc );

            OnPropertyChanged( nameof( LastLocation ) );
        }

        public MapPoint? LastLocation => MapPoints.Any() ? MapPoints.Last() : null;
    }
}
