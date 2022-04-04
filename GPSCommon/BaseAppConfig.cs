using System;
using System.Reflection;
using J4JSoftware.GPSLocator;
using Serilog;
using Serilog.Events;

namespace J4JSoftware.GPSCommon;

public class BaseAppConfig : DeviceConfig
{
    private LogEventLevel _minLevel = LogEventLevel.Verbose;
    private string? _launchPage;

    protected BaseAppConfig()
    {
        AppVersion = Assembly.GetExecutingAssembly().GetName().Version;
    }

    public override void Initialize( IDeviceContext context )
    {
        base.Initialize( context );

        if( context is not ICommonAppContext appContext )
            return;

        HelpLink = appContext.HelpLink;
    }

    public Version? AppVersion { get; }

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

    public string HelpLink { get; protected set; } = string.Empty;
}