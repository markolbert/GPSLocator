using System.ComponentModel;

namespace J4JSoftware.InReach;

public interface IAppConfig : IInReachConfig, INotifyPropertyChanged
{
    bool IsValid { get; set; }
}
