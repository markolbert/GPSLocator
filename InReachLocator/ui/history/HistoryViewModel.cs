using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace J4JSoftware.InReach
{
    public class HistoryViewModel : LocationMapViewModel
    {
        private readonly IInReachConfig _config;

        private DateTimeOffset? _minDate;
        private DateTimeOffset? _startDate;
        private DateTimeOffset? _endDate;
        private Location? _selectedLocation;
        private bool _deferUpdateLocations;

        public HistoryViewModel(
            IInReachConfig config,
            IJ4JLogger logger
        )
        : base(logger)
        {
            _config = config;
            
            IsActive = true;
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
                MinimumStartDate = _endDate?.AddDays( -31 );
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
            if( StartDate == null || EndDate == null )
                return;

            var request = new HistoryRequest( _config, Logger )
            {
                Start = StartDate.Value.UtcDateTime, End = EndDate.Value.UtcDateTime
            };

            var result = Task.Run( async () => await request.ExecuteAsync() );
            var items = result.Result?.HistoryItems;

            if( items == null || items.Count == 0 )
            {
                if( request.LastError != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", request.LastError.ToString() );
                else Logger.Error( "Invalid configuration" );

                return;
            }

            SelectedLocation = null;
            Locations.Clear();

            foreach( var item in items )
            {
                Locations.Add( item );
            }
        }

        public ObservableCollection<LocationMessage> Locations { get; } = new();

        public Location? SelectedLocation
        {
            get => _selectedLocation;

            set
            {
                SetProperty( ref _selectedLocation, value );

                if( value == null )
                    return;

                LocationUrl = $"https://maps.google.com?q={value.Coordinate!.Latitude},{value.Coordinate.Longitude}";
            }
        }
    }
}
