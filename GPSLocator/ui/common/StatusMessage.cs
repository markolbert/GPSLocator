using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.GPSLocator
{
    public record StatusMessage( string Text, StatusMessageType Type = StatusMessageType.Normal, int MsPause = 0 );
}
