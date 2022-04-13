using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GPSLocator;

public class SelectablePointViewModel<TAppConfig> : LocationMapViewModel<TAppConfig>
    where TAppConfig : BaseAppConfig
{
    private MapPoint? _selectedPoint;
    private bool _refreshEnabled;

    public SelectablePointViewModel(
        DisplayedPoints displayedPoints,
        BaseAppViewModel<TAppConfig> appViewModel,
        CachedLocations cachedLocations,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base(displayedPoints, appViewModel, statusMessages, logger)
    {
        CachedLocations = cachedLocations;
        CachedLocations.CacheChanged += CachedLocationsOnCacheChanged;
        CachedLocations.TimeSpanChanged += CachedLocationsOnTimeSpanChanged;

        RefreshCommand = new RelayCommand(RefreshHandler);
        SetMapPoint = new RelayCommand<MapPoint>( SetMapPointHandler );
    }

    private void CachedLocationsOnTimeSpanChanged( object? sender, EventArgs e )
    {
        OnPropertyChanged(nameof(EndDate));
        OnPropertyChanged( nameof( StartDate ) );
    }

    protected CachedLocations CachedLocations { get; }

    public void OnPageActivated()
    {
        RefreshEnabled = true;
        Messenger.Send(new MapViewModelMessage<TAppConfig>(this), "primary");
        UpdateLocations();
        SelectedPoint = DisplayedPoints.FirstOrDefault();
        OnPropertyChanged( nameof( DisplayedPoints ) );

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
        if (!AppViewModel.Configuration.IsValid)
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
        DisplayedPoints.Clear();

        if( mapPoint != null )
            DisplayedPoints.Add( mapPoint );
    }

    protected override void OnMapChanged( MapPoint? center, BoundingBox? boundingBox )
    {
        base.OnMapChanged( center, boundingBox );

        SelectedPoint = center;
    }

    public MapPoint? SelectedPoint
    {
        get => _selectedPoint;
        private set => SetProperty( ref _selectedPoint, value );
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

        DisplayedPoints.Clear();
        UpdateLocations();

        RefreshEnabled = true;
    }

    protected void UpdateLocations()
    {
        OnPropertyChanged( nameof( RetrievedPoints ) );
        OnPropertyChanged( nameof( NumRetrieved ) );
    }

    public bool RefreshEnabled
    {
        get => _refreshEnabled;
        set => SetProperty(ref _refreshEnabled, value);
    }

    protected virtual bool IncludeLocation( MapPoint mapPoint ) => true;

    public IEnumerable<MapPoint> RetrievedPoints
    {
        get
        {
            foreach( var mapPoint in CachedLocations.MapPoints.Where( IncludeLocation ) )
            {
                yield return mapPoint;
            }
        }
    }

    public int NumRetrieved => CachedLocations.MapPoints.Count( IncludeLocation );

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