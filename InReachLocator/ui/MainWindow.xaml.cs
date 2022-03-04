using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.InReach
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private record DisplayControl( UserControl Control, Action<ContentControl> ContainerConfigurator );

        private readonly Stack<DisplayControl> _displayControls = new();
        private readonly ContentControl? _placeholder;
        private readonly IJ4JLogger _logger;

        public MainWindow()
        {
            this.InitializeComponent();

            Title = "InReach Locator";

            _logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
            _logger.SetLoggedType( GetType() );

            // find and store the content control
            for( var idx = 0; idx < VisualTreeHelper.GetChildrenCount( Content ); idx++ )
            {
                var child = VisualTreeHelper.GetChild( Content, idx );
                if( child is not ContentControl curCtl || curCtl.Name != "ContentPlaceholder" )
                    continue;

                _placeholder = curCtl;
                break;
            }

            if( _placeholder != null )
                SetContentControl( new HomeControl(),
                                   x =>
                                   {
                                       x.HorizontalAlignment = HorizontalAlignment.Center;
                                       x.VerticalAlignment = VerticalAlignment.Center;
                                   } );
            else _logger.Fatal( "Could not find content placeholder" );
        }

        public void SetContentControl(
            UserControl control,
            Action<ContentControl> containerConfigurator
        )
        {
            if( _placeholder == null )
            {
                _logger.Error( "Could not display new {0} because content placeholder is undefined",
                               control.GetType() );
                return;
            }

            var newControlContext = new DisplayControl( control, containerConfigurator );
            _displayControls.Push( newControlContext );

            _placeholder.Content = control;
            containerConfigurator( _placeholder );
        }

        public void PopContentControl()
        {
            if( _placeholder == null )
            {
                _logger.Error( "Could not revert to prior content because content placeholder is undefined" );
                return;
            }

            if( _displayControls.Count == 0 )
            {
                _logger.Error( "Could not revert to prior content because stack is empty" );
                return;
            }

            // top of stack is currently displayed UserControl, which we no longer need
            _displayControls.Pop();

            var prior = _displayControls.Peek();

            _placeholder.Content = prior.Control;
            prior.ContainerConfigurator( _placeholder );
        }

        private void MainWindow_OnSizeChanged( object sender, WindowSizeChangedEventArgs args )
        {
            WeakReferenceMessenger.Default.Send( new SizeMessage(args.Size), "mainwindow" );
        }
    }
}
