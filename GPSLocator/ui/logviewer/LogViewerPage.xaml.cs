using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LogViewerPage : Page
{
    public const string PageName = "LogViewer";

    public LogViewerPage()
    {
        this.InitializeComponent();

        ViewModel = App.Current.Host.Services.GetRequiredService<LogViewerViewModel>();

        Loaded += OnLoaded;
    }

    private LogViewerViewModel ViewModel { get; }

    private void OnLoaded( object sender, RoutedEventArgs e )
    {
        ViewModel.OnPageActivated();
    }
}