using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Syncfusion.UI.Xaml.DataGrid;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.InReach
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        public HistoryPage()
        {
            this.InitializeComponent();

            DataContext = App.Current.Host.Services.GetRequiredService<HistoryViewModel>();

            WeakReferenceMessenger.Default.Register<HistoryPage, LocationTypeMessage, string>(
                this,
                "LocationTypeChanged",
                LocationTypeChangedHandler );
        }

        private void LocationTypeChangedHandler( HistoryPage recipient, LocationTypeMessage message )
        {
            var dataRow = LocationsGrid.RowGenerator.Items[message.RowNumber];

            if( dataRow.Element is not DataGridRowControl rowControl )
                return;

            var rowStyle = message.LocationType switch
            {
                LocationType.Pushpin => OuterContainer.Resources["PushpinRowStyle"] as Style,
                LocationType.RoutePoint => OuterContainer.Resources["RoutePointRowStyle"] as Style,
                _ => null
            };

            if( rowStyle == null )
                return;

            rowControl.Style = rowStyle;
        }
    }
}
