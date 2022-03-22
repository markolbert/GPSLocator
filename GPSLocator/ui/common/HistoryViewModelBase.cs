using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator;

public class HistoryViewModelBase : LocationMapViewModel
{
    private DateTimeOffset _endDate;
    private double _daysBack = 7;
    private MapPoint? _selectedPoint;

    protected HistoryViewModelBase(
        IJ4JLogger logger
    )
        : base(logger)
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds( 1 ) };
        timer.Tick += Timer_Tick;
        timer.Start();

        DaysBack = AppViewModel.Configuration.DefaultDaysBack;
        EndDate = DateTimeOffset.Now;
    }

    public void OnPageActivated()
    {
        RefreshHandler();
    }

    private void Timer_Tick(object? sender, object e)
    {
        EndDate = DateTimeOffset.Now;
    }

    protected override void RefreshHandler()
    {
        if( !AppViewModel.Configuration.IsValid )
        {
            StatusMessages.Message( "Invalid configuration" ).Urgent().Enqueue();
            StatusMessages.DisplayReady();
            return;
        }

        var request = new HistoryRequest<Location>( AppViewModel.Configuration, Logger )
        {
            Start = StartDate.UtcDateTime, End = EndDate.UtcDateTime
        };

        ExecuteRequest(request, OnHistoryRequestStatusChanged);
    }

    private void OnHistoryRequestStatusChanged(RequestEventArgs<History<Location>> args)
    {
        switch (args.RequestEvent)
        {
            case RequestEvent.Started:
                StatusMessages.Message("Updating history").Indeterminate().Important().Display();
                RefreshEnabled = false;

                break;

            case RequestEvent.Succeeded:
                OnSucceeded(args);
                break;

            case RequestEvent.Aborted:
                OnAborted(args);
                break;

            default:
                throw new InvalidEnumArgumentException($"Unsupported {typeof(RequestEvent)} '{args.RequestEvent}'");
        }
    }

    private void OnSucceeded( RequestEventArgs<History<Location>> args )
    {
        StatusMessages.Message( "Retrieved history" ).Important().Enqueue();
        StatusMessages.DisplayReady();

        AddLocations( args.Response!.Result!.HistoryItems
                          .Where( LocationFilter ) );


        RefreshEnabled = true;
    }

    private void OnAborted( RequestEventArgs<History<Location>> args )
    {
        StatusMessages.Message( $"Retrieval failed ({( args.ErrorMessage ?? "Unspecified error" )})" )
                      .Important()
                      .Enqueue();

        StatusMessages.DisplayReady();

        if( args.Response?.Error != null )
            Logger.Error<string>( "Invalid configuration, message was '{0}'", args.Response.Error.Description );
        else Logger.Error( "Invalid configuration" );

        RefreshEnabled = true;
    }

    protected virtual bool LocationFilter( Location toCheck ) => true;

    public DateTimeOffset StartDate => _endDate.DateTime.AddDays(-DaysBack);

    public DateTimeOffset EndDate
    {
        get => _endDate;

        set
        {
            SetProperty(ref _endDate, value);
            OnPropertyChanged(nameof(StartDate));
        }
    }

    public double DaysBack
    {
        get => _daysBack;

        set
        {
            SetProperty(ref _daysBack, value);
            OnPropertyChanged(nameof(StartDate));
        }
    }

    public MapPoint? SelectedPoint
    {
        get => _selectedPoint;

        set
        {
            if (_selectedPoint?.DisplayOnMap == MapPointDisplay.Transitory)
                _selectedPoint.DisplayOnMap = MapPointDisplay.DoNotDisplay;

            SetProperty(ref _selectedPoint, value);

            if (_selectedPoint == null)
                return;

            if (_selectedPoint.DisplayOnMap == MapPointDisplay.DoNotDisplay
             && _selectedPoint.DeviceLocation.Coordinate.Latitude != 0
             && _selectedPoint.DeviceLocation.Coordinate.Longitude != 0)
                _selectedPoint.DisplayOnMap = MapPointDisplay.Transitory;

            if (_selectedPoint.DisplayOnMap != MapPointDisplay.DoNotDisplay)
                MapCenter = _selectedPoint.DisplayPoint;
        }
    }
}