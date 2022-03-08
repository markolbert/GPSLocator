using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.InReach
{
    public class MapPoint : ObservableObject
    {
        private AnnotatedLocationType _selectedLocType;

        public MapPoint(
            ILocation inReachLocation,
            AnnotatedLocationType initialLocType
        )
        {
            InReachLocation = inReachLocation;

            DisplayPoint = new MapControl.Location( inReachLocation.Coordinate.Latitude,
                                                inReachLocation.Coordinate.Longitude );

            Label =
                $"{inReachLocation.Coordinate.Latitude}, {inReachLocation.Coordinate.Longitude}\n{inReachLocation.Timestamp}";

            _selectedLocType = initialLocType;
        }

        public ILocation InReachLocation { get; }
        public MapControl.Location DisplayPoint { get; }

        public AnnotatedLocationType SelectedLocationType
        {
            get => _selectedLocType;
            set => SetProperty( ref _selectedLocType, value );
        }

        public string Label { get; }
    }
}
