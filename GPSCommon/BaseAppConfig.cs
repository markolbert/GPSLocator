using System;
using System.Collections.Generic;
using System.Reflection;
using J4JSoftware.GPSLocator;
using Serilog.Events;

namespace J4JSoftware.GPSCommon;

public class BaseAppConfig : DeviceConfig, IBaseAppConfig
{
    private LogEventLevel _minLevel = LogEventLevel.Verbose;
    private string? _launchPage;
    private bool _hideInvalidLoc;

    protected BaseAppConfig()
    {
        AppVersion = Assembly.GetExecutingAssembly().GetName().Version;
    }

    public override void Initialize( IDeviceContext context )
    {
        base.Initialize( context );

        foreach (var mapService in MapCredentials)
        {
            mapService.EncryptedApiKey.Logger = context.Logger;
            mapService.EncryptedApiKey.Protector = context.Protector;
        }

        if ( context is not ICommonAppContext appContext )
            return;

        HelpLink = appContext.HelpLink;
    }

    public Version? AppVersion { get; }

    public List<MapServiceCredentials> MapCredentials { get; set; } = new();

    public LogEventLevel MinimumLogLevel
    {
        get => _minLevel;
        set => SetProperty( ref _minLevel, value );
    }

    public string? LaunchPage
    {
        get => _launchPage;
        set => SetProperty( ref _launchPage, value );
    }

    public bool HideInvalidLocations
    {
        get => _hideInvalidLoc;
        set => SetProperty(ref _hideInvalidLoc, value);
    }

    public string HelpLink { get; protected set; } = string.Empty;
}