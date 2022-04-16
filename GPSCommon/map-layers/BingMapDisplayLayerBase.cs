using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapControl;

namespace J4JSoftware.GPSCommon
{
    public class BingMapDisplayLayer : KeyedMapDisplayLayer
    {
        public BingMapDisplayLayer()
            : base( "Bing Maps" )
        {
            ApiKey.ClearText = "Ameqf9cCPjqwawIcun91toGQ-F85jSDu8-XyEFeHEdTDr60dV9ySmZt800aHj6PS";
        }

        public BingMapsTileLayer.MapMode MapMode { get; set; } = BingMapsTileLayer.MapMode.Road;
        public string? Culture { get; set; }

        public override string SourceName => $"{Description} ({MapMode})";

        public override MapTileLayer GetMapLayer()
        {
            BingMapsTileLayer.ApiKey = ApiKey.ClearText;

            return new BingMapsTileLayer() { Mode = MapMode, Culture = Culture };
        }
    }
}
