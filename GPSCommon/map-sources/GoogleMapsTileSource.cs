using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.GPSCommon
{
    public class GoogleMapsTileSource : TileSource
    {
        private readonly IJ4JLogger _logger;

        public GoogleMapsTileSource(
            IJ4JLogger logger
        )
        {
            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public override Uri GetUri( int x, int y, int zoomLevel )
        {
            return base.GetUri( x, y, zoomLevel );
        }

        public override Task<ImageSource> LoadImageAsync( int x, int y, int zoomLevel )
        {
            return base.LoadImageAsync( x, y, zoomLevel );
        }
    }
}
