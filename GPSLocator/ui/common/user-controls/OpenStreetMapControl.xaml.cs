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

        WeakReferenceMessenger.Default.Register<OpenStreetMapControl, MapViewModelMessage, string>(this, "primary", ViewModelChangedHandler);
    }

    private void ViewModelChangedHandler(OpenStreetMapControl recipient, MapViewModelMessage message )
    {
        ViewModel = message.ViewModel;
    }

    public LocationMapViewModel ViewModel { get; private set; }

    private void UIElement_OnDoubleTapped( object sender, DoubleTappedRoutedEventArgs e )
    {
    }
}