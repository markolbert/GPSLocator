using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.InReach
{
    public class MapLocation : Location, IMapLocation
    {
        public const double MinimumDelta = 0.000001;

        private MapControl.Location _mapPoint = new( 0, 0 );
        private LocationType _locationType = LocationType.Unspecified;
        private string _label = string.Empty;

        public MapControl.Location MapPoint
        {
            get
            {
                if( Math.Abs( _mapPoint.Latitude - Coordinate.Latitude )
                 + Math.Abs( _mapPoint.Longitude - Coordinate.Longitude )
                >= MinimumDelta )
                {
                    _mapPoint = new MapControl.Location(Coordinate.Latitude, Coordinate.Longitude);
                    OnPropertyChanged();

                    Label = $"{Coordinate.SimpleDisplay}\n{Timestamp}";
                }

                return _mapPoint;
            }
        }

        public LocationType LocationType
        {
            get => _locationType;

            set
            {
                _locationType = value;
                OnPropertyChanged();

                OnPropertyChanged( nameof( Label ) );
            }
        }

        public string Label
        {
            get => _label;

            set
            {
                _label = value;
                OnPropertyChanged();
            }
        }
    }
}
