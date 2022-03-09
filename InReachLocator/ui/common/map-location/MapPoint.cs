using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public class Collection : BindingList<MapPoint>
        {
            protected override void OnAddingNew( AddingNewEventArgs e )
            {
                base.OnAddingNew( e );

                if( e.NewObject is not MapPoint mapPoint )
                    throw new InvalidCastException(
                        $"Trying to add a {e.NewObject?.GetType()} but a {typeof( MapPoint )} is required" );

                mapPoint.BelongsTo = this;
            }

            protected override void RemoveItem( int index )
            {
                this[ index ].BelongsTo = null;

                base.RemoveItem( index );
            }

            protected override void SetItem( int index, MapPoint item )
            {
                base.SetItem( index, item );

                this[ index ].BelongsTo = this;
            }

            protected override void ClearItems()
            {
                foreach( var item in this )
                {
                    item.BelongsTo = null;
                }

                base.ClearItems();
            }
        }

        private LocationType _selectedLocType = LocationType.Unspecified;

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

        public MapPoint.Collection? BelongsTo { get; private set; }

        public ILocation InReachLocation { get; }
        public MapControl.Location DisplayPoint { get; }

        public LocationType SelectedLocationType
        {
            get => _selectedLocType;
            set => SetProperty( ref _selectedLocType, value );
        }

        public string Label { get; }
    }
}
