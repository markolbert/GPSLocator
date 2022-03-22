using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

    public void OnPageActivated()
    {
        RefreshHandler();
    }

    protected override void RefreshHandler()
    {
        if( !AppViewModel.Configuration.IsValid )
        {
            StatusMessages.Message("Invalid configuration").Urgent().Enqueue();
            StatusMessages.DisplayReady();

            return;
        }

        var request = new LastKnownLocationRequest<Location>( AppViewModel.Configuration, Logger );

        ExecuteRequest(request, OnRequestStatusChanged);
    }

    private void OnRequestStatusChanged(RequestEventArgs<LastKnownLocation<Location>> args )
    {
        switch( args.RequestEvent )
        {
            case RequestEvent.Started:
                StatusMessages.Message( "Updating last known location" ).Indeterminate().Important().Display();
                RefreshEnabled = false;

                break;

            case RequestEvent.Succeeded:
                OnSucceeded( args );
                break;

            case RequestEvent.Aborted:
                OnAborted( args );
                break;

            default:
                throw new InvalidEnumArgumentException( $"Unsupported {typeof( RequestEvent )} '{args.RequestEvent}'" );
        }
    }

    private void OnSucceeded( RequestEventArgs<LastKnownLocation<Location>> args )
    {
        if( args.Response?.Result?.Locations.Any() ?? false )
        {
            StatusMessages.Message("Last known location updated").Enqueue();
            StatusMessages.DisplayReady();

            ClearMappedPoints();

            var lastLoc = args.Response.Result.Locations[0];
            lastLoc.CompassHeadings = AppViewModel.Configuration.UseCompassHeadings;
            lastLoc.ImperialUnits = AppViewModel.Configuration.UseImperialUnits;

            var mapPoint = AddLocation(lastLoc);
            mapPoint.DisplayOnMap = MapPointDisplay.Fixed;

            LastKnownPoint = MappedPoints[0];
        }
        else StatusMessages.Message("No last known location").Important().Enqueue();

        StatusMessages.DisplayReady();

        RefreshEnabled = true;
    }

    private void OnAborted(RequestEventArgs<LastKnownLocation<Location>> args)
    {
        StatusMessages.Message($"Retrieval failed ({(args.ErrorMessage ?? "Unspecified error")})")
                      .Important()
                      .Enqueue();

        StatusMessages.DisplayReady();

        if (args.Response?.Error != null)
            Logger.Error<string>("Invalid configuration, message was '{0}'", args.Response.Error.Description);
        else Logger.Error("Invalid configuration");

        RefreshEnabled = true;
    }

    public MapPoint? LastKnownPoint
    {
        get => _lastKnownPoint;
        set => SetProperty( ref _lastKnownPoint, value );
    }
}