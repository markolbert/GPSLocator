using J4JSoftware.GPSCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
#pragma warning disable CS8618

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator;

public sealed partial class OpenStreetMapControl : UserControl
{
    public OpenStreetMapControl()
    {
        this.InitializeComponent();

        ViewModel = App.Current.Host.Services.GetRequiredService<RetrievedPoints>();

        WeakReferenceMessenger.Default.Register<OpenStreetMapControl, MapLayerChangedMessage, string>(
            this,
            "primary",
            MapLayerChangedHandler );
    }

    private void MapLayerChangedHandler( OpenStreetMapControl recipient, MapLayerChangedMessage message )
    {
        TheMap.MapLayer = message.MapService.MapLayer;
    }

    private RetrievedPoints ViewModel { get; }
}