using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Events;

namespace J4JSoftware.InReach
{
    public class AppConfig : InReachConfig
    {
        public bool UseImperialUnits { get; set; } = true;
        public bool UseCompassHeadings { get; set; } = true;
        public LogEventLevel MinimumLogLevel { get; set; } = LogEventLevel.Verbose;
    }
}
