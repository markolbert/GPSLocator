using System;
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
    private DateTimeOffset _endDate;
    private double _daysBack = 7;
    private MapPoint? _pointOnMap;
    private bool _refreshEnabled;

    public SelectablePointViewModel(
        BaseAppViewModel<TAppConfig> appViewModel,
        CachedLocations cachedLocations,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base(appViewModel, statusMessages, logger)
    {
        CachedLocations = cachedLocations;
        CachedLocations.CacheChanged += CachedLocationsOnCacheChanged;
        CachedLocations.TimeSpanChanged += CachedLocationsOnTimeSpanChanged;

        RefreshCommand = new RelayCommand(RefreshHandler);

        Messenger.Send(new MapViewModelMessage<TAppConfig>(this), "primary");
    }

    private void CachedLocationsOnTimeSpanChanged( object? sender, EventArgs e )
    {
        OnPropertyChanged(nameof(EndDate));
        OnPropertyChanged( nameof( StartDate ) );
    }

    protected CachedLocations CachedLocations { get; }

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

    public MapPoint? PointOnMap
    {
        get => _pointOnMap;

        set
        {
            if (_pointOnMap?.MapLocation.Latitude == 0
             && _pointOnMap?.MapLocation.Longitude == 0)
                value = null;

            SetProperty(ref _pointOnMap, value);
            DisplayedPoints.Clear();

            if (value != null)
                DisplayedPoints.Add(value);
        }
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

        UpdateLocations();

        RefreshEnabled = true;
    }

    protected void UpdateLocations()
    {
        DisplayedPoints.Clear();
        RetrievedPoints.Clear();

        foreach (var location in CachedLocations.Locations.Where(IncludeLocation))
        {
            RetrievedPoints.Add(new MapPoint(location));
        }

        // not sure why I have to do this...
        OnPropertyChanged( nameof( RetrievedPoints ) );
    }

    public bool RefreshEnabled
    {
        get => _refreshEnabled;
        set => SetProperty(ref _refreshEnabled, value);
    }

    protected virtual bool IncludeLocation(ILocation location) => true;

    public ObservableCollection<MapPoint> RetrievedPoints { get; } = new();

    public DateTimeOffset StartDate => CachedLocations.StartDate;
    public DateTimeOffset EndDate => CachedLocations.EndDate;

    public double DaysBack
    {
        get => _daysBack;

        set
        {
            if (value <= 0)
            {
                Logger.Warning("Ignoring attempt to set DaysBack to a number <= 0 ({0})", value);
                return;
            }

            SetProperty(ref _daysBack, value);
            CachedLocations.DaysBack = value;

            OnPropertyChanged(nameof(StartDate));
        }
    }

}