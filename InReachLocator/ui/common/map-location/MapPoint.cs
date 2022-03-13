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
    public class MapPoint
    {
        public event EventHandler<bool>? Display;

        private bool _displayOnMap;

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

        public bool DisplayOnMap
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
