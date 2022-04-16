using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MapControl;

namespace J4JSoftware.GPSCommon
{
    public interface IMapDisplayLayer
    {
        MapTileLayer GetMapLayer();
    }

    public interface IKeylessMapDisplayLayer : IMapDisplayLayer
    {
        string UriFormat { get; }
        string SourceName { get; }
        string Description { get; }
    }

    public record KeylessMapDisplayLayerBase : IKeylessMapDisplayLayer
    {
        protected KeylessMapDisplayLayerBase(
            string uriFormat,
            string srcName,
            string description
        )
        {
            UriFormat = uriFormat;
            SourceName = srcName;
            Description = description;
        }

        public string UriFormat { get; init; }
        public string SourceName { get; init; }
        public string Description { get; init; }

        public virtual MapTileLayer GetMapLayer() =>
            new()
            {
                TileSource = new TileSource() { UriFormat = UriFormat },
                SourceName = SourceName,
                Description = Description
            };
    }

    public record OpenStreetMapDisplayLayer() : KeylessMapDisplayLayerBase( "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                                                              "OpenStreetMap",
                                                              "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)" );

    public record OpenTopoMapDisplayLayer() : KeylessMapDisplayLayerBase( "https://tile.opentopomap.org/{z}/{x}/{y}.png",
                                                            "OpenTopoMap",
                                                            "© [OpenTopoMap](https://opentopomap.org/) © [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)" );
}
