using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GPSLocator;

public class SelectablePointViewModel : LocationMapViewModel<AppConfig>
{
    private bool _refreshEnabled;

    public SelectablePointViewModel(
        RetrievedPoints displayedPoints,
        IAppConfig appConfig,
        CachedLocations cachedLocations,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base(displayedPoints, statusMessages, logger)
    {
        AppConfig = appConfig;

        CachedLocations = cachedLocations;
        CachedLocations.CacheChanged += CachedLocationsOnCacheChanged;
        CachedLocations.TimeSpanChanged += CachedLocationsOnTimeSpanChanged;

        RefreshCommand = new RelayCommand(RefreshHandler);
        SetMapPoint = new RelayCommand<MapPoint>( SetMapPointHandler );

        if( !CachedLocations.Executed )
            DaysBack = AppConfig.DefaultDaysBack;
    }

    protected IAppConfig AppConfig { get; }

    private void CachedLocationsOnTimeSpanChanged( object? sender, EventArgs e )
    {
        OnPropertyChanged(nameof(EndDate));
        OnPropertyChanged( nameof( StartDate ) );
    }

    protected CachedLocations CachedLocations { get; }

    public virtual void OnPageActivated()
    {
        RefreshEnabled = true;
        Messenger.Send(new MapViewModelMessage<AppConfig>(this), "primary");
        UpdateLocations();

        if( CachedLocations.Executed )
        {
            Logger.Information( "Locations already cached, skipping retrieval" );
            return;
        }

        RefreshHandler();
    }

    public RelayCommand RefreshCommand { get; }

    protected void RefreshHandler()
    {
        if (!AppConfig.IsValid)
        {
            StatusMessages.Message("Invalid configuration").Urgent().Enqueue();
            StatusMessages.DisplayReady();
            return;
        }

        CachedLocations.BeginUpdate();
    }

    public RelayCommand<MapPoint> SetMapPoint { get; }

    private void SetMapPointHandler( MapPoint? mapPoint )
    {
        RetrievedPoints.UnselectAll();

        if( mapPoint != null )
            RetrievedPoints.Select( mapPoint );
    }

    private void CachedLocationsOnCacheChanged(object? sender, CachedLocationEventArgs e)
    {
        switch (e.Phase)
        {
            case RequestEvent.Started:
                StatusMessages.Message("Beginning locations retrieval").Indeterminate().Display();
                break;

            case RequestEvent.Aborted:
                StatusMessages.Message("Locations retrieval failed").Urgent().Enqueue();
                StatusMessages.DisplayReady();

                break;

            case RequestEvent.Succeeded:
                OnCacheUpdated();
                break;

            default:
                Logger.Error("Unsupported RequestEvent value '{0}'", e.Phase);
                break;
        }
    }

    private void OnCacheUpdated()
    {
        StatusMessages.Message("Retrieved history").Important().Enqueue();
        StatusMessages.DisplayReady();

        var mapPts = InitializeMapPoints(CachedLocations.MapPoints);

        RetrievedPoints.Clear();
        RetrievedPoints.AddRange(mapPts);
        UpdateLocations();

        RefreshEnabled = true;
    }

    protected virtual List<MapPoint> InitializeMapPoints( List<MapPoint> mapPoints ) => mapPoints;

    protected void UpdateLocations()
    {
        OnPropertyChanged( nameof( RetrievedPoints ) );
        OnPropertyChanged( nameof( NumDisplayable ) );
    }

    public bool RefreshEnabled
    {
        get => _refreshEnabled;
        set => SetProperty(ref _refreshEnabled, value);
    }

    public int NumDisplayable => RetrievedPoints.NumDisplayable;

    public DateTimeOffset StartDate => CachedLocations.StartDate;
    public DateTimeOffset EndDate => CachedLocations.EndDate;

    public double DaysBack
    {
        get => CachedLocations.DaysBack;

        set
        {
            if (value <= 0)
            {
                Logger.Warning("Ignoring attempt to set DaysBack to a number <= 0 ({0})", value);
                return;
            }

            CachedLocations.DaysBack = value;
        }
    }

}