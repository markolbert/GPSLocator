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
            IAppConfig configuration,
            AnnotatedLocationType.Choices locationTypeChoices,
            IJ4JLogger logger
        )
        : base(configuration, locationTypeChoices, logger)
        {
            var dQueue = DispatcherQueue.GetForCurrentThread();
            dQueue.TryEnqueue( async () =>
            {
                await RefreshHandlerAsync();
            } );

            RefreshCommand = new AsyncRelayCommand( RefreshHandlerAsync );
        }

        public AsyncRelayCommand RefreshCommand { get; }

        private async Task RefreshHandlerAsync()
        {
            if( !Configuration.IsValid )
                return;

            var request = new LastKnownLocationRequest<Location>( Configuration, Logger );
            var result = await request.ExecuteAsync();

            if( result == null || result.Locations.Count == 0 )
            {
                if( request.LastError != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", request.LastError.ToString() );
                else Logger.Error( "Invalid configuration" );

                return;
            }

            ClearMapLocations();

            AddPushpin( result.Locations[ 0 ] );

            OnPropertyChanged( nameof( LastLocation ) );
        }

        public MapPoint? LastLocation => MapPoints.Any() ? MapPoints.Last() : null;
    }
}
