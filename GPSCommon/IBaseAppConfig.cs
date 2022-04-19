using System;
using System.Collections.Generic;
using J4JSoftware.GPSLocator;
using Serilog.Events;

namespace J4JSoftware.GPSCommon;

public interface IBaseAppConfig : IDeviceConfig
{
    public Version? AppVersion { get; }

    public List<MapServiceCredentials> MapCredentials { get; set; }

    public LogEventLevel MinimumLogLevel { get; set; }
    public string? LaunchPage { get; set; }
    public bool HideInvalidLocations { get; set; }
    public string HelpLink { get; }
}
