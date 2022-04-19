using J4JSoftware.WindowsAppUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSCommon;

public sealed partial class MapViewControl : UserControl
{
    public MapViewControl()
    {
        this.InitializeComponent();

        ViewModel = J4JServices.Default.GetRequiredService<MapViewModel>();

        WeakReferenceMessenger.Default.Register<MapViewControl, MapLayerChangedMessage, string>(
            this,
            "primary",
            MapLayerChangedHandler );

        TheMap.MapLayer = ViewModel.MapLayerGenerator?.GetMapTileLayer();
    }

    private void MapLayerChangedHandler( MapViewControl recipient, MapLayerChangedMessage message )
    {
        TheMap.MapLayer = ViewModel.MapLayerGenerator?.GetMapTileLayer();
    }

    private MapViewModel ViewModel { get; }
}