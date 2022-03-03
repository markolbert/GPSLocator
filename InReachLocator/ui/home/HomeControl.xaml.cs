using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.InReach
{
    public sealed partial class HomeControl : UserControl
    {
        public HomeControl()
        {
            this.InitializeComponent();

            ViewModel = App.Current.Host.Services.GetRequiredService<HomeViewModel>();
        }

        internal HomeViewModel ViewModel { get; }
    }
}
