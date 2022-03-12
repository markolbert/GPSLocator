using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.InReach
{
    public class HistoryViewModel : LocationMapViewModel
    {
        private DateTimeOffset _endDate;
        private double _daysBack = 7;
        private bool _refreshEnabled;

        public HistoryViewModel(
            IJ4JLogger logger
        )
        : base(logger)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes( 1 ) };
            timer.Tick += Timer_Tick;
            timer.Start();

            RefreshCommand = new AsyncRelayCommand( RefreshCommandHandler );
            EndDate = DateTimeOffset.Now;

            ChangeSelectedMapPointsCommand = new RelayCommand<LocationType>( ChangeSelectedMapPointsHandler );

            SelectedMapPoints.CollectionChanged += SelectedMapPoints_CollectionChanged;

            RefreshCommandHandler();
        }

        private void Timer_Tick(object? sender, object e)
        {
            EndDate = DateTimeOffset.Now;
            RefreshEnabled = true;
        }

        public DateTimeOffset StartDate => _endDate.DateTime.AddDays( -DaysBack );

        public DateTimeOffset EndDate
        {
            get => _endDate;

            set
            {
                SetProperty( ref _endDate, value );
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public double DaysBack
        {
            get => _daysBack;

            set
            {
                SetProperty( ref _daysBack, value );
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public AsyncRelayCommand RefreshCommand { get; }

        private async Task RefreshCommandHandler()
        {
            if( !Configuration.IsValid )
                return;

            var request = new HistoryRequest<Location>( Configuration.Configuration, Logger )
            {
                Start = StartDate.UtcDateTime, End = EndDate.UtcDateTime
            };

            var response = await request.ExecuteAsync();

            if( !response.Succeeded )
            {
                if( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );

                return;
            }

            DeferUpdatingMapCenter = true;
            ClearMapLocations();

            foreach( var mapLocation in response.Result!.HistoryItems )
            {
                mapLocation.CompassHeadings = Configuration.UseCompassHeadings;
                mapLocation.ImperialUnits = Configuration.UseImperialUnits;

                AddUnspecifiedPoint( mapLocation );
            }

            DeferUpdatingMapCenter = false;
            UpdateMapCenter();
            RefreshEnabled = false;
        }

        public bool RefreshEnabled
        {
            get => _refreshEnabled;
            set => SetProperty(ref _refreshEnabled, value);
        }

        public ObservableCollection<object> SelectedMapPoints { get; }  = new();

        private void SelectedMapPoints_CollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
        {
            OnPropertyChanged(nameof(MapPointToDisplay));
        }

        public RelayCommand<LocationType> ChangeSelectedMapPointsCommand { get; }

        private void ChangeSelectedMapPointsHandler( LocationType locationType )
        {
            if( SelectedMapPoints.Count == 0 )
                return;

            foreach( var mapPoint in SelectedMapPoints
                                    .Cast<MapPoint>()
                                    .ToList() )
            {
                mapPoint.SelectedLocationType = locationType;

                var rowNum = MapPoints.IndexOf( mapPoint );

                WeakReferenceMessenger.Default.Send( new LocationTypeMessage( rowNum + 1, locationType ),
                                                     "LocationTypeChanged" );
            }

            SelectedMapPoints.Clear();

            OnPropertyChanged( nameof( Pushpins ) );
            OnPropertyChanged( nameof( Route ) );

            UpdateMapCenter();
        }

        public MapPoint? MapPointToDisplay => SelectedMapPoints.Count == 1 ? (MapPoint?) SelectedMapPoints[ 0 ] : null;

        protected override bool IncludeLocationType( LocationType locationType ) =>
            locationType != LocationType.Unspecified;

        protected override void OnMapPointsChanged()
        {
            if( DeferUpdatingMapCenter )
                return;

            base.OnMapPointsChanged();

            var mapChanged = false;

            if( MapPoints.Any( x => x.SelectedLocationType == LocationType.Pushpin ) )
            {
                OnPropertyChanged(nameof(Pushpins));
                mapChanged = true;
            }

            if( MapPoints.Any( x => x.SelectedLocationType == LocationType.RoutePoint ) )
            {
                OnPropertyChanged(nameof(Route));
                mapChanged = true;
            }

            if( mapChanged )
                UpdateMapCenter();
        }
    }
}
