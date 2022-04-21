using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.StaticMaps.Request;
using GoogleApi.Entities.Maps.StaticMaps.Request.Enums;
using GoogleApi.Entities.Maps.StaticMaps.Response;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using MapScale = GoogleApi.Entities.Maps.StaticMaps.Request.Enums.MapScale;

namespace J4JSoftware.GPSCommon
{
    public class GoogleTileImageLoader : J4JTileImageLoader
    {
        private readonly DispatcherQueue _dQueue;

        private readonly MapBase _map;
        private readonly string _apiKey;

        public GoogleTileImageLoader(
            MapBase map,
            string apiKey,
            IJ4JLogger logger 
            )
        :base(logger)
        {
            _map = map;
            _apiKey = apiKey;

            _dQueue = DispatcherQueue.GetForCurrentThread();
        }

        // it's a violation of Google's terms of service to cache their map imagery
        protected override bool CacheTiles() => false;

        protected override BoundingBox? GetBoundingBox() =>
            _map.ViewRectToBoundingBox( new Rect( 0, 0, _map.ActualWidth, _map.ActualHeight ) );

        protected async override Task LoadTile( Tile tile )
        {
            if (CurrentBoundingBox == null)
            {
                Logger.Error("Could not convert view box to geographic bounding box");
                return;
            }

            var mapReq = new StaticMapsRequest()
            {
                Center = new GoogleApi.Entities.Maps.Common.Location(
                    new Coordinate( CurrentBoundingBox.Center.Latitude, CurrentBoundingBox.Center.Latitude ) ),
                Key = _apiKey,
                ZoomLevel = (byte) tile.ZoomLevel,
                Size = new MapSize( 256, 256 ),
                Scale = MapScale.Large,
                Styles = new MapStyle[]
                {
                    new MapStyle()
                    {
                        Element = StyleElement.All, 
                        Feature = StyleFeature.All, 
                        Style = new StyleRule()
                    }
                }
            };

            StaticMapsResponse? mapResp = null;

            try
            {
                mapResp = GoogleApi.GoogleMaps.StaticMaps.Query(mapReq);
            }
            catch (Exception ex)
            {
                Logger.Error<string>("Failed to validate access to Google Maps API, message was '{0}'", ex.Message);
                return;
            }

            try
            {
                _dQueue.TryEnqueue( async () =>
                {
                    var imgSrc = await ImageLoader.LoadImageAsync( mapResp.Buffer );

                    if( imgSrc != null )
                        await SetTileImage( tile, imgSrc );
                } );
            }
            catch (Exception ex)
            {
                int i = 0;
                i++;
            }
        }

        private Task SetTileImage(Tile tile, ImageSource imgSrc )
        {
            var tcs = new TaskCompletionSource();

            void Callback()
            {
                try
                {
                    tile.SetImage( imgSrc );
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            if (_dQueue.TryEnqueue(DispatcherQueuePriority.Low, Callback))
                return tcs.Task;

            tile.Pending = true;
            tcs.TrySetResult();

            return tcs.Task;
        }
    }
}
