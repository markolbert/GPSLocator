using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Syncfusion.UI.Xaml.DataGrid;

namespace J4JSoftware.InReach
{
    public class HistoryRowStyleSelector : StyleSelector
    {
        protected override Style SelectStyleCore( object item, DependencyObject container )
        {
            if( item is not DataRowBase row )
                return base.SelectStyleCore( item, container );

            if( row.RowData is not MapPoint mapPoint )
                return base.SelectStyleCore( item, container );

            return mapPoint.SelectedLocationType switch
            {
                LocationType.Pushpin => App.Current.Resources[ "PushpinRowStyle" ] as Style
                 ?? base.SelectStyleCore( item, container ),

                LocationType.RoutePoint => App.Current.Resources[ "RoutePointRowStyle" ] as Style
                 ?? base.SelectStyleCore( item, container ),
                
                _ => base.SelectStyleCore( item, container )
            };
        }
    }
}
