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
        protected override bool LocationFilter(Location toCheck) => toCheck.HasMessage || !_mustHaveMessages;
    }
}
