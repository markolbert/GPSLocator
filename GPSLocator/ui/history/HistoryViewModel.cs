using System;
using System.Linq;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator
{
    public class HistoryViewModel : HistoryViewModelBase
    {
        private bool _mustHaveMessages;

        public HistoryViewModel(
            IJ4JLogger logger
        )
        : base(logger)
        {
            MapPointSetCommand = new RelayCommand<MapPoint>( MapPointSetHandler );
            ClearMapCommand = new RelayCommand( ClearMapHandler );
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

                if( SelectedPoint != null && !SelectedPoint.DeviceLocation.HasMessage )
                    SelectedPoint = null;

                var locations = AllPoints.Where( x => x.DeviceLocation.HasMessage )
                                          .Select( x => x.DeviceLocation )
                                          .ToList();
                AddLocations( locations );

                UpdateMapCenter();
            }
        }

        protected override bool LocationFilter( Location toCheck ) => toCheck.HasMessage || !_mustHaveMessages;

        public RelayCommand ClearMapCommand { get; }

        private void ClearMapHandler()
        {
            MappedPoints.Clear();
            UpdateMapCenter();
        }

        public RelayCommand<MapPoint> MapPointSetCommand { get; }

        private void MapPointSetHandler( MapPoint? selectedPoint )
        {
            if( selectedPoint == null
            || ( selectedPoint.DeviceLocation.Coordinate.Latitude == 0
                && selectedPoint.DeviceLocation.Coordinate.Longitude == 0 ) )
                return;

            selectedPoint.DisplayOnMap = selectedPoint.DisplayOnMap switch
            {
                MapPointDisplay.DoNotDisplay => MapPointDisplay.Fixed,
                _ => MapPointDisplay.DoNotDisplay
            };

            if( selectedPoint.DisplayOnMap != MapPointDisplay.DoNotDisplay )
                MapCenter = selectedPoint.DisplayPoint;
        }
    }
}
