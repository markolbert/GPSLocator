using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.InReach
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            ViewModel = App.Current.Host.Services.GetRequiredService<SettingsViewModel>();

            this.Loaded += SettingsPage_Loaded;
        }

        private SettingsViewModel ViewModel { get; }

        private void SettingsPage_Loaded( object sender, RoutedEventArgs e )
        {
            ViewModel.OnLoaded();
        }
    }
}
