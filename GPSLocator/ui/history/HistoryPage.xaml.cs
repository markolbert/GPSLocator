using System;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls.Primitives;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        public const string PageName = "History";

        private readonly IJ4JLogger _logger;

        public HistoryPage()
        {
            InitializeComponent();

            ViewModel = App.Current.Host.Services.GetRequiredService<HistoryViewModel>();

            _logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
            _logger.SetLoggedType(GetType());

            Loaded += OnLoaded;
        }

        public HistoryViewModel ViewModel { get; }

        private async void OnLoaded( object sender, RoutedEventArgs e )
        {
            ViewModel.OnPageActivated();
        }

        private void LocationsGrid_OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if( !ViewModel.SelectedPoint?.DeviceLocation.HasMessage ?? false )
                return;

            // have to wrap this in try/catch because when you return to the page
            // from another page it will throw an exception
            try
            {
                FlyoutBase.ShowAttachedFlyout( (FrameworkElement) sender );
            }
            catch(Exception ex )
            {
                // ignored
                _logger.Error<string>( "Flyout failed to open, message was '{0}'", ex.Message );
            }
        }
    }
}
