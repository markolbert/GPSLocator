﻿using System;
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
            {
                StatusMessage.Send( "Invalid configuration", StatusMessageType.Urgent );
                return;
            }

            var pBar = StatusMessage.SendWithIndeterminateProgressBar("Updating last known location");

            var request = new LastKnownLocationRequest<Location>( Configuration.Configuration, Logger );
            var response = await request.ExecuteAsync();

            ProgressBarMessage.EndProgressBar(pBar);

            if ( !response.Succeeded || response.Result!.Locations.Count == 0 )
            {
                StatusMessage.Send( "Couldn't retrieve last known location", StatusMessageType.Important );

                if ( response.Error != null )
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

            StatusMessage.Send( "Ready" );
        }

        public MapPoint? LastLocation => MapPoints.Any() ? MapPoints.Last() : null;
    }
}
