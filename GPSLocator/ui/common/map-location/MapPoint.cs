using System;

namespace J4JSoftware.GPSLocator
{
    public class MapPoint
    {
        public event EventHandler<MapPointDisplay>? Display;

        private MapPointDisplay _displayOnMap;

        public MapPoint(
            ILocation deviceLocation
        )
        {
            DeviceLocation = deviceLocation;

            DisplayPoint = new MapControl.Location( deviceLocation.Coordinate.Latitude,
                                                deviceLocation.Coordinate.Longitude );

            Label =
                $"{deviceLocation.Coordinate.Latitude}, {deviceLocation.Coordinate.Longitude}\n{deviceLocation.Timestamp}";
        }

        public ILocation DeviceLocation { get; }
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
