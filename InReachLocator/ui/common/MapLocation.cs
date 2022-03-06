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
                    _label = $"{Coordinate.SimpleDisplay}\n{Timestamp}";
                }

                return _mapPoint;
            }
        }

        public LocationType LocationType { get; set; } = LocationType.Unspecified;
        public string Label => LocationType == InReach.LocationType.Pushpin ? _label : string.Empty;
    }
}
