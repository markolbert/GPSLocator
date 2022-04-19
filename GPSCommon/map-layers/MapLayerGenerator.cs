using System;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSCommon
{
    public abstract partial class MapLayerGenerator
    {
        protected MapLayerGenerator(
            MapType mapType,
            string label,
            string? copyrightText,
            Uri? copyrightUri,
            MapServiceType? reqdSvcType,
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( GetType() );

            var mapSvcType = mapType.GetMapServiceType();

            if( reqdSvcType == null && mapSvcType != null )
            {
                Logger.Error( "{0} should not be decorated with a {1} but is", mapType, typeof( MapServiceAttribute ) );
                IsValid = false;
            }

            if( reqdSvcType != null && mapSvcType != reqdSvcType )
            {
                Logger.Error( "{0} must be decorated with a {1} but isn't", mapType, typeof( MapServiceAttribute ) );
                IsValid = false;
            }

            MapType = mapType;
            Label = label;
            CopyrightText = copyrightText;
            CopyrightUri = copyrightUri;
        }

        protected IJ4JLogger Logger { get; }

        public bool IsValid { get; protected set; } = true;

        public MapType MapType { get; }
        public string Label { get; }
        public string? CopyrightText { get; }
        public Uri? CopyrightUri { get; }

        public abstract MapControl.MapTileLayer? GetMapTileLayer();
    }
}
