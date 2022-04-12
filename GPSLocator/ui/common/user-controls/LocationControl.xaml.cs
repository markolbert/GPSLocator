using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using J4JSoftware.GPSCommon;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator;

public sealed partial class LocationControl : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public LocationControl()
    {
        this.InitializeComponent();

        DataContextChanged += LocationControl_DataContextChanged;
    }

    private MapPoint? ViewModel { get; set; }

    private void LocationControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if( args.NewValue is not MapPoint viewModel )
            return;

        ViewModel = viewModel;
        OnPropertyChanged( nameof( ViewModel ) );
    }

    private void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }
}