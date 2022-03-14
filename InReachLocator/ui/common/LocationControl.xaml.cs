using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using J4JSoftware.InReach.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.InReach
{
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
}
