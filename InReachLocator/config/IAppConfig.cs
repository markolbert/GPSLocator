using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using J4JSoftware.Logging;
using Serilog.Events;

namespace J4JSoftware.InReach;

public interface IAppConfig : IInReachConfig
{
    bool IsValid { get; set; }
    IndexedLogEvent.Collection LogEvents { get; }
}
