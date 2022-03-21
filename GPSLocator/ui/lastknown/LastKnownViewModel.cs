using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator;

public class LastKnownViewModel : LocationMapViewModel
{
    private MapPoint? _lastKnownPoint;

    public LastKnownViewModel(
        IJ4JLogger logger
    )
        : base( logger )
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
            MessageQueue.Default.Message("Invalid configuration").Urgent().Enqueue();
            MessageQueue.Default.Ready();

            return;
        }

        var request = new LastKnownLocationRequest<Location>( AppViewModel.Configuration, Logger );

        DeviceResponse<LastKnownLocation<Location>>? response = null;
        await Task.Run( async () => { response = await ExecuteRequestAsync( request, OnRequestStatusChanged ); } );

        if( !response!.Succeeded || response.Result!.Locations.Count == 0 )
        {
            MessageQueue.Default.Message( "Couldn't retrieve last known location" ).Important().Enqueue();

            if( response.Error?.Description != null )
                MessageQueue.Default.Message( response.Error.Description ).Important().Enqueue();

            MessageQueue.Default.Ready();

            if( response.Error != null )
                Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
            else Logger.Error( "Invalid configuration" );
        }
        else
        {
            ClearMappedPoints();

            var lastLoc = response.Result.Locations[ 0 ];
            lastLoc.CompassHeadings = AppViewModel.Configuration.UseCompassHeadings;
            lastLoc.ImperialUnits = AppViewModel.Configuration.UseImperialUnits;

            var mapPoint = AddLocation( lastLoc );
            mapPoint.DisplayOnMap = MapPointDisplay.Fixed;

            LastKnownPoint = MappedPoints[ 0 ];

            MessageQueue.Default.Message( "Retrieved last known location" ).Important().Enqueue();
            MessageQueue.Default.Ready();
        }

    }

    private void OnRequestStatusChanged( DeviceRequestEventArgs args )
    {
        var error = args.Message ?? "Unspecified error";

        ( string msg, bool pBar, bool enabled ) = args.RequestEvent switch
        {
            RequestEvent.Started => ( "Updating last known location", true, false ),
            RequestEvent.Succeeded => ( "Last known location updated", false, true ),
            RequestEvent.Aborted => ( $"Update failed: {error}", false, true ),
            _ => throw new InvalidEnumArgumentException( $"Unsupported RequestEvent '{args.RequestEvent}'" )
        };

        if( pBar )
            MessageQueue.Default.Message( msg ).Indeterminate().Enqueue();
        else
            MessageQueue.Default.Message(msg).Enqueue();

        RefreshEnabled = enabled;
    }

    public MapPoint? LastKnownPoint
    {
        get => _lastKnownPoint;
        set => SetProperty( ref _lastKnownPoint, value );
    }
}