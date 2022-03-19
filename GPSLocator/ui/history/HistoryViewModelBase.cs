using System;
using System.Collections.Generic;
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
                AppViewModel.SetStatusMessage( "Invalid configuration", StatusMessageType.Urgent );
                return;
            }

            var request = new HistoryRequest<Location>( AppViewModel.Configuration, Logger )
            {
                Start = StartDate.UtcDateTime, End = EndDate.UtcDateTime
            };

            var response = await ExecuteRequestAsync( request, OnHistoryRequestStarted, OnHistoryRequestEnded );

            if( response!.Succeeded )
            {
                AddLocations(response.Result!.HistoryItems
                                     .Where(LocationFilter));

                await AppViewModel.SetStatusMessagesAsync(1000,
                                                          new StatusMessage(
                                                              "Retrieved history",
                                                              StatusMessageType.Important),
                                                          new StatusMessage("Ready"));

            }
            else
            {
                var mesgs = new List<StatusMessage>
                {
                    new StatusMessage( "Couldn't retrieve history", StatusMessageType.Important ),
                    new StatusMessage( "Ready" )
                };

                if( response.Error?.Description != null )
                    mesgs.Insert( 1, new StatusMessage( response.Error.Description, StatusMessageType.Important ) );

                await AppViewModel.SetStatusMessagesAsync( 2000, mesgs );

                if( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );
            }

            RefreshEnabled = true;
        }

        private void OnHistoryRequestStarted()
        {
            AppViewModel.SetStatusMessage("Updating history");
            AppViewModel.IndeterminateVisibility = Visibility.Visible;
            RefreshEnabled = false;
        }

        private void OnHistoryRequestEnded()
        {
            AppViewModel.IndeterminateVisibility = Visibility.Collapsed;
            RefreshEnabled = true;
        }

        protected virtual bool LocationFilter( Location toCheck ) => true;

        //private void RequestStarted(object? sender, EventArgs e)
        //{
        //    _dQueue.TryEnqueue( OnRequestStarted );
        //}

        //protected virtual void OnRequestStarted()
        //{
        //}

        //private void RequestEnded(object? sender, EventArgs e)
        //{
        //    _dQueue.TryEnqueue( OnRequestEnded );
        //}

        //protected virtual void OnRequestEnded()
        //{

        //}

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
