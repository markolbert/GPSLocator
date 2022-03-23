using System;
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

        public HistoryPage()
        {
            InitializeComponent();

            ViewModel = App.Current.Host.Services.GetRequiredService<HistoryViewModel>();

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
            catch
            {
                // ignored
            }
        }
    }
}
