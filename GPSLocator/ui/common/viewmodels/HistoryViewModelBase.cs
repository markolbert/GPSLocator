﻿using System;
using System.ComponentModel;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator;

public class HistoryViewModelBase : LocationMapViewModel
{
    private bool _initialized;
    private DateTimeOffset _endDate;
    private double _daysBack = 7;

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
        if( _initialized )
            return;

        RefreshHandler();

        _initialized = true;
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

        ClearDisplayedPoints();

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
}