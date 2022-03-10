using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

public interface IAppConfig : IInReachConfig
{
    bool IsValid { get; set; }
    ObservableCollection<NetEventArgs> LogEvents { get; }
    void ClearLogEvents();
}
