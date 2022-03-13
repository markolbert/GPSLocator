using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;

namespace J4JSoftware.InReach
{
    public class MapPoint : ObservableObject
    {
        private LocationType _locType = LocationType.Unspecified;

        public MapPoint(
            ILocation inReachLocation
        )
        {
            InReachLocation = inReachLocation;

            DisplayPoint = new MapControl.Location( inReachLocation.Coordinate.Latitude,
                                                inReachLocation.Coordinate.Longitude );

            Label =
                $"{inReachLocation.Coordinate.Latitude}, {inReachLocation.Coordinate.Longitude}\n{inReachLocation.Timestamp}";
        }

        public ILocation InReachLocation { get; }
        public MapControl.Location DisplayPoint { get; }

        public LocationType LocationType
        {
            get => _locType;

            set
            {
                SetProperty( ref _locType, value );
                OnPropertyChanged( nameof( LocationTypeText ) );
            }
        }

        public string LocationTypeText =>
            _locType switch
            {
                LocationType.Pushpin => "Pushpin",
                LocationType.RoutePoint => "Route Point",
                LocationType.Unspecified => string.Empty,
                _ => "Unknown"
            };

        public string Label { get; }
    }
}
