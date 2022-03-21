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

namespace J4JSoftware.GPSLocator
{
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

        public async Task OnPageActivated()
        {
            await RefreshHandlerAsync();
        }

        private void Timer_Tick(object? sender, object e)
        {
            EndDate = DateTimeOffset.Now;
        }

        protected override async Task RefreshHandlerAsync()
        {
            if( !AppViewModel.Configuration.IsValid )
            {
                MessageQueue.Default.Message( "Invalid configuration" ).Urgent().Enqueue();
                MessageQueue.Default.Ready();
                return;
            }

            var request = new HistoryRequest<Location>( AppViewModel.Configuration, Logger )
            {
                Start = StartDate.UtcDateTime, End = EndDate.UtcDateTime
            };

            var response = await ExecuteRequestAsync( request, OnHistoryRequestStatusChanged );

            if( response!.Succeeded )
            {
                AddLocations(response.Result!.HistoryItems
                                     .Where(LocationFilter));

                MessageQueue.Default.Message( "Retrieved history" ).Important().Enqueue();
                MessageQueue.Default.Ready();
            }
            else
            {
                MessageQueue.Default.Message( "Couldn't retrieve history" ).Important().Enqueue();

                if( response.Error?.Description != null )
                    MessageQueue.Default.Message( response.Error.Description ).Important().Enqueue();
                
                MessageQueue.Default.Ready();

                if( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );
            }

            RefreshEnabled = true;
        }

        private void OnHistoryRequestStatusChanged( DeviceRequestEventArgs args )
        {
            var error = args.Message ?? "Unspecified error";

            ( string msg, bool pBar, bool enabled ) = args.RequestEvent switch
            {
                RequestEvent.Started => ( "Updating history", true, false ),
                RequestEvent.Succeeded => ( "History updated", false, true ),
                RequestEvent.Aborted => ( $"Update failed: {error}", false, true ),
                _ => throw new InvalidEnumArgumentException( $"Unsupported RequestEvent '{args.RequestEvent}'" )
            };

            if( pBar )
                MessageQueue.Default.Message( msg ).Enqueue();
            else MessageQueue.Default.Message( msg ).Indeterminate().Enqueue();

            RefreshEnabled = enabled;
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
}
