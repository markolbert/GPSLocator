﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.InReach
{
    public sealed partial class LastKnownControl : UserControl
    {
        public LastKnownControl()
        {
            this.InitializeComponent();

            ViewModel = App.Current.Host.Services.GetRequiredService<LastKnownViewModel>();
        }

        public LastKnownViewModel ViewModel { get; set; }

        private void OnSizeChanged( object sender, SizeChangedEventArgs e )
        {
            ViewModel.GridHeight = e.NewSize.Height;
            ViewModel.GridWidth = e.NewSize.Width;
        }
    }
}
