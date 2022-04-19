using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using J4JSoftware.GPSCommon;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator;

public sealed partial class LocationControl : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly IAppConfig _appConfig;

    public LocationControl()
    {
        this.InitializeComponent();

        DataContextChanged += LocationControl_DataContextChanged;

        _appConfig = J4JServices.Default.GetRequiredService<IAppConfig>();
    }

    private MapPoint ViewModel { get; set; } = new( 0, 0, DateTime.Now );

    private void LocationControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if( args.NewValue is not MapPoint viewModel )
            return;

        ViewModel = viewModel;
        ViewModel.CompassHeadings = _appConfig.UseCompassHeadings;
        ViewModel.ImperialUnits = _appConfig.UseImperialUnits;
        
        OnPropertyChanged( nameof( ViewModel ) );
    }

    private void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }
}