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
        private bool _mustHaveMessages;
        private MapPoint? _selectedPoint;

        public HistoryViewModel(
            IJ4JLogger logger
        )
        : base(logger)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds( 1 ) };
            timer.Tick += Timer_Tick;
            timer.Start();

            EndDate = DateTimeOffset.Now;

            RefreshCommand = new AsyncRelayCommand( RefreshCommandHandler );
            MapPointCommand = new RelayCommand<MapPoint>( MapPointHandler );
            ClearMapCommand = new RelayCommand( ClearMapHandler );
        }

        public async Task OnPageActivated()
        {
            await RefreshCommandHandler();
        }

        private void Timer_Tick(object? sender, object e)
        {
            EndDate = DateTimeOffset.Now;
        }

        public AsyncRelayCommand RefreshCommand { get; }

        private async Task RefreshCommandHandler()
        {
            if( !Configuration.IsValid )
            {
                StatusMessage.Send("Invalid configuration", StatusMessageType.Urgent);
                return;
            }

            RefreshEnabled = false;

            var request = new HistoryRequest<Location>( Configuration.Configuration, Logger )
            {
                Start = StartDate.UtcDateTime, End = EndDate.UtcDateTime
            };

            var pBar = StatusMessage.SendWithIndeterminateProgressBar("Updating history");

            var response = await request.ExecuteAsync();

            ProgressBarMessage.EndProgressBar(pBar);

            if ( !response.Succeeded )
            {
                StatusMessage.Send("Couldn't retrieve history", StatusMessageType.Important);

                if ( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );

                RefreshEnabled = true;

                return;
            }

            AddLocations( response.Result!.HistoryItems
                                  .Where( x => !MustHaveMessages || x.HasMessage ) );

            WeakReferenceMessenger.Default.Send( new UpdateColumnWidthMessage(), "primary" );

            RefreshEnabled = true;
            StatusMessage.Send("Ready");
        }

        public bool RefreshEnabled
        {
            get => _refreshEnabled;
            set => SetProperty(ref _refreshEnabled, value);
        }

        public bool MustHaveMessages
        {
            get => _mustHaveMessages;

            set
            {
                var changed = value != _mustHaveMessages;

                SetProperty( ref _mustHaveMessages, value );

                if( !changed || !_mustHaveMessages )
                    return;

                if( SelectedPoint != null && !SelectedPoint.InReachLocation.HasMessage )
                    SelectedPoint = null;

                var locations = AllPoints.Where( x => x.InReachLocation.HasMessage )
                                          .Select( x => x.InReachLocation )
                                          .ToList();
                AddLocations( locations );

                UpdateMapCenter();
            }
        }

        public RelayCommand ClearMapCommand { get; }

        private void ClearMapHandler()
        {
            MappedPoints.Clear();
            UpdateMapCenter();
        }

        public RelayCommand<MapPoint> MapPointCommand { get; }

        private void MapPointHandler( MapPoint? selectedPoint )
        {
            if( selectedPoint == null )
                return;

            selectedPoint.DisplayOnMap = !selectedPoint.DisplayOnMap;

            if( selectedPoint.DisplayOnMap )
                MapCenter = selectedPoint.DisplayPoint;
        }

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
                SetProperty( ref _selectedPoint, value );

                if( _selectedPoint == null )
                    return;

                if( _selectedPoint.DisplayOnMap )
                    MapCenter = _selectedPoint.DisplayPoint;
            }
        }
    }
}
