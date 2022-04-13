using System.ComponentModel;
using System.Linq;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;
using MapControl;

namespace J4JSoftware.GPSLocator;

public class LastKnownViewModel : LocationMapViewModel<AppConfig>
{
    private bool _refreshEnabled;
    private MapPoint? _lastKnownPoint;

    public LastKnownViewModel(
        DisplayedPoints displayedPoints,
        AppViewModel appViewModel,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( displayedPoints, appViewModel, statusMessages, logger )
    {
        Messenger.Send( new MapViewModelMessage<AppConfig>( this ), "primary" );
    }

    public void OnPageActivated()
    {
        if (!AppViewModel.Configuration.IsValid)
        {
            StatusMessages.Message("Invalid configuration").Urgent().Enqueue();
            StatusMessages.DisplayReady();

            return;
        }

        var request = new LastKnownLocationRequest<Location>(AppViewModel.Configuration, Logger);

        ExecuteRequest(request, OnRequestStatusChanged);
    }

    protected override void OnMapChanged( MapPoint? center, BoundingBox? boundingBox )
    {
        base.OnMapChanged( center, boundingBox );

        LastKnownPoint = center;
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

            var lastLoc = args.Response.Result.Locations[0];

            DisplayedPoints.Add( new MapPoint( lastLoc )
            {
                CompassHeadings = AppViewModel.Configuration.UseCompassHeadings,
                ImperialUnits = AppViewModel.Configuration.UseImperialUnits
            } );
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

    public bool RefreshEnabled
    {
        get => _refreshEnabled;
        set => SetProperty(ref _refreshEnabled, value);
    }

    public MapPoint? LastKnownPoint
    {
        get => _lastKnownPoint;
        set => SetProperty( ref _lastKnownPoint, value );
    }
}