using J4JSoftware.DependencyInjection;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class HistoryPage : Page
{
    private readonly IJ4JLogger _logger;

    public HistoryPage()
    {
        InitializeComponent();

        ViewModel = J4JServices.Default.GetRequiredService<HistoryViewModel>();

        _logger = J4JServices.Default.GetRequiredService<IJ4JLogger>();
        _logger.SetLoggedType(GetType());

        Loaded += OnLoaded;
    }

    public HistoryViewModel ViewModel { get; }

    private void OnLoaded( object sender, RoutedEventArgs e )
    {
        ViewModel.OnPageActivated();
    }

    private void LocationsGrid_OnTapped( object sender, TappedRoutedEventArgs e )
    {
        if (sender is not ListView uiElement)
            return;

        if (LocationsGrid.SelectedItem is not MapPoint mapPoint || !mapPoint.HasMessage)
            return;

        FlyoutBase.ShowAttachedFlyout(uiElement);
    }
}