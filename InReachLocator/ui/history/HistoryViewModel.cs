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
        private MapPoint? _selectedMapPoint;
        private bool _deferUpdateLocations;

        public HistoryViewModel(
            IInReachConfig config,
            AnnotatedLocationType.Choices locationTypeChoices,
            IJ4JLogger logger
        )
        : base(locationTypeChoices, logger)
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

            var request = new HistoryRequest<LocationMessage>( _config, Logger )
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

            SelectedMapPoint = null;
            ClearMapLocations();

            foreach( var mapLocation in mapLocations )
            {
                AddMapLocation( new MapPoint(mapLocation) );
            }
        }

        public MapPoint? SelectedMapPoint
        {
            get => _selectedMapPoint;
            set => SetProperty( ref _selectedMapPoint, value );
        }
    }
}
