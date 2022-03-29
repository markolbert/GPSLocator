using System;
using System.Linq;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator
{
    public class SelectablePointViewModel : HistoryViewModelBase
    {
        private MapPoint? _selectedPoint;

        public SelectablePointViewModel(
            IJ4JLogger logger
        )
        : base(logger)
        {
        }

        public MapPoint? SelectedPoint
        {
            get => _selectedPoint;

            set
            {
                if (_selectedPoint?.DeviceLocation.Coordinate.Latitude != 0
                && _selectedPoint?.DeviceLocation.Coordinate.Longitude != 0)
                    value = null;

                SetProperty(ref _selectedPoint, value);
                DisplayedPoints.Clear();

                if( value != null )
                    DisplayedPoints.Add( value );
            }
        }
    }
}
