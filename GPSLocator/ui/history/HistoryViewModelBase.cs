using System;
using System.Linq;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator
{
    public class HistoryViewModelBase : LocationMapViewModel
    {
        private readonly DispatcherQueue _dQueue;

        private DateTimeOffset _endDate;
        private double _daysBack = 7;
        private MapPoint? _selectedPoint;

        public HistoryViewModelBase(
            IJ4JLogger logger
        )
        : base(logger)
        {
            _dQueue = DispatcherQueue.GetForCurrentThread();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds( 1 ) };
            timer.Tick += Timer_Tick;
            timer.Start();

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
                AppViewModel.SetStatusMessage("Invalid configuration", StatusMessageType.Urgent);
                return;
            }

            var request = new HistoryRequest<Location>(AppViewModel.Configuration, Logger)
            {
                Start = StartDate.UtcDateTime,
                End = EndDate.UtcDateTime
            };

            request.Started += RequestStarted;
            request.Ended += RequestEnded;

            DeviceResponse<History<Location>>? response = null;
            await Task.Run(async () =>
            {
                response = await request.ExecuteAsync();
            });

            if ( !response!.Succeeded )
            {
                AppViewModel.SetStatusMessage("Couldn't retrieve history", StatusMessageType.Important);

                if ( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );

                RefreshEnabled = true;

                return;
            }

            AddLocations( response.Result!.HistoryItems
                                  .Where( LocationFilter ) );

            RefreshEnabled = true;
            AppViewModel.SetStatusMessage("Ready");
        }

        protected virtual bool LocationFilter( Location toCheck ) => true;

        private void RequestStarted(object? sender, EventArgs e)
        {
            _dQueue.TryEnqueue( OnRequestStarted );
        }

        protected virtual void OnRequestStarted()
        {
        }

        private void RequestEnded(object? sender, EventArgs e)
        {
            _dQueue.TryEnqueue( OnRequestEnded );
        }

        protected virtual void OnRequestEnded()
        {

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
                if( _selectedPoint?.DisplayOnMap == MapPointDisplay.Transitory )
                    _selectedPoint.DisplayOnMap = MapPointDisplay.DoNotDisplay;

                SetProperty( ref _selectedPoint, value );

                if( _selectedPoint == null )
                    return;

                if( _selectedPoint.DisplayOnMap == MapPointDisplay.DoNotDisplay
                   && _selectedPoint.DeviceLocation.Coordinate.Latitude != 0
                   && _selectedPoint.DeviceLocation.Coordinate.Longitude != 0 )
                    _selectedPoint.DisplayOnMap = MapPointDisplay.Transitory;

                if( _selectedPoint.DisplayOnMap != MapPointDisplay.DoNotDisplay )
                    MapCenter = _selectedPoint.DisplayPoint;
            }
        }
    }
}
