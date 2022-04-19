using System.ComponentModel;
using System.Linq;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public class LastKnownViewModel : LocationMapViewModel<AppConfig>
{
    private readonly IAppConfig _appConfig;

    private bool _refreshEnabled;
    private MapPoint? _lastKnownPoint;

    public LastKnownViewModel(
        IAppConfig appConfig,
        MapViewModel displayedPoints,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( displayedPoints, statusMessages, logger )
    {
        _appConfig = appConfig;

        Messenger.Send( new MapViewModelMessage<AppConfig>( this ), "primary" );
    }

    public void OnPageActivated()
    {
        if (!_appConfig.IsValid)
        {
            StatusMessages.Message("Invalid configuration").Urgent().Enqueue();
            StatusMessages.DisplayReady();

            return;
        }

        var request = new LastKnownLocationRequest<Location>(_appConfig, Logger);

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

            var lastLoc = args.Response.Result.Locations[0];

            LastKnownPoint = MapViewModel.Add( lastLoc );

            LastKnownPoint.CompassHeadings = _appConfig.UseCompassHeadings;
            LastKnownPoint.ImperialUnits = _appConfig.UseImperialUnits;

            MapViewModel.Select( LastKnownPoint );
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