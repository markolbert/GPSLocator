using System;

namespace J4JSoftware.InReach
{
    public class MapPoint
    {
        public event EventHandler<MapPointDisplay>? Display;

        private MapPointDisplay _displayOnMap;

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

        public MapPointDisplay DisplayOnMap
        {
            get => _displayOnMap;

            set
            {
                var changed = _displayOnMap != value;

                _displayOnMap = value;

                if( changed )
                    Display?.Invoke( this, _displayOnMap );
            }
        }

        public bool IsSelected { get; set; }
        public string Label { get; }
    }
}
