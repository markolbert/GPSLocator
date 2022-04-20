using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleApi.Entities.Common;
using GoogleApiCommon = GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.StaticMaps.Request;
using MapControl;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.GPSCommon
{
    public class GoogleTileSource : TileSource
    {
        public static GoogleTileSource? Create( IBaseAppConfig appConfig )
        {
            var credentials = appConfig.MapCredentials
                                       .FirstOrDefault( x => x.ServiceType == MapServiceType.GoogleMaps );

            if( credentials == null || credentials.ApiKey == null )
                return null;

            return new GoogleTileSource( credentials.ApiKey );
        }

        private readonly string _apiKey;

        private GoogleTileSource(
            string apiKey
        )
        {
            _apiKey = apiKey;
        }

        public override Uri GetUri( int x, int y, int zoomLevel )
        {
            var center = new GoogleApiCommon.Location( new Coordinate( x, y ) );

            var req = new StaticMapsRequest() { Center = center, ZoomLevel = (byte) zoomLevel };

            return req.GetUri();
        }

        public override Task<ImageSource> LoadImageAsync( int x, int y, int zoomLevel )
        {
            var center = new GoogleApiCommon.Location(new Coordinate(x, y));
            return null;
        }

        private GoogleApiCommon.Location GetLocation( int x, int y )
        {
            return null;
        }
    }
}
