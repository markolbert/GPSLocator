using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using J4JSoftware.GPSLocator.Annotations;

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

    private Location ViewModel { get; set; } = new();

    private void LocationControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if( args.NewValue is not Location viewModel )
            return;

        ViewModel = viewModel;
        OnPropertyChanged( nameof( ViewModel ) );
    }

    [ NotifyPropertyChangedInvocator ]
    private void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }
}