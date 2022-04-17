using System.Collections.Generic;
using J4JSoftware.GPSLocator;

namespace J4JSoftware.GPSCommon;

public interface ICommonAppContext : IDeviceContext
{
    string HelpLink { get; }
}
