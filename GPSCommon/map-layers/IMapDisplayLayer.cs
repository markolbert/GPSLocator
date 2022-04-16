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
        public string SourceName { get; }
        public string Description { get; }

        MapTileLayer GetMapLayer();
    }
}
