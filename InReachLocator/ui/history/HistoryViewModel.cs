using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace J4JSoftware.InReach
{
    public class HistoryViewModel : LocationMapViewModel
    {
        private DateTimeOffset? _minDate;
        private DateTimeOffset? _startDate;
        private DateTimeOffset? _endDate;
        private bool _deferUpdateLocations;

        public HistoryViewModel(
            IAppConfig config,
            IJ4JLogger logger
        )
        : base(config, logger)
        {
            ChangeSelectedMapPointsCommand = new RelayCommand<LocationType>( ChangeSelectedMapPointsHandler );

            SelectedMapPoints.CollectionChanged += SelectedMapPoints_CollectionChanged;
        }

        public DateTimeOffset? StartDate
        {
            get => _startDate;

            set
            {
                SetProperty( ref _startDate, value );

                if( !_deferUpdateLocations)
                    UpdateLocations();
            }
        }

        public DateTimeOffset? EndDate
        {
            get => _endDate;

            set
            {
                SetProperty( ref _endDate, value );

                _deferUpdateLocations = true;
                MinimumStartDate = _endDate?.AddDays( -32 );
                _deferUpdateLocations = false;

                UpdateLocations();
            }
        }

        public DateTimeOffset? MinimumStartDate
        {
            get => _minDate;

            set
            {
                SetProperty( ref _minDate, value );

                if( _minDate > _startDate )
                    StartDate = value;
            }
        }

        private void UpdateLocations()
        {
            if( !Configuration.IsValid || StartDate == null || EndDate == null )
                return;

            var request = new HistoryRequest<Location>( Configuration, Logger )
            {
                Start = StartDate.Value.UtcDateTime, End = EndDate.Value.UtcDateTime
            };

            var result = Task.Run( async () => await request.ExecuteAsync() );
            var mapLocations = result.Result?.HistoryItems;

            if( mapLocations == null || mapLocations.Count == 0 )
            {
                if( request.LastError != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", request.LastError.ToString() );
                else Logger.Error( "Invalid configuration" );

                return;
            }

            DeferUpdatingMapCenter = true;
            ClearMapLocations();

            foreach( var mapLocation in mapLocations )
            {
                AddMapLocation( mapLocation );
            }

            DeferUpdatingMapCenter = false;
            UpdateMapCenter();
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

            foreach( var mapPoint in SelectedMapPoints.Cast<MapPoint>() )
            {
                mapPoint.SelectedLocationType = locationType;

                var rowIndex = MapPoints.IndexOf( mapPoint );
                WeakReferenceMessenger.Default.Send( new LocationTypeMessage( rowIndex + 1, locationType ),
                                                     "LocationTypeChanged" );
            }
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
