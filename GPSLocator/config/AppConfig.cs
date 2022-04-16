using System;
using System.Collections.Generic;
using System.Reflection;
using J4JSoftware.GPSCommon;
using Serilog;
using Serilog.Events;

namespace J4JSoftware.GPSLocator;

public class AppConfig : BaseAppConfig
{
    private bool _useImperial;
    private bool _useCompass;
    private string? _defaultCallback;
    private int _defaultDaysBack;

    public AppConfig(
        IEnumerable<IMapDisplayLayer> mapLayers
    )
        : base( mapLayers )
    {
        HelpLink = "https://www.jumpforjoysoftware.com/gpslocator-user-docs/";
    }

    public override void Initialize( IDeviceContext context )
    {
        base.Initialize( context );

        if( context is not GpsLocatorContext locatorContext )
            return;

        MaxSmsLength = locatorContext.MaxSmsLength;
    }

    public bool UseImperialUnits
    {
        get=> _useImperial;
        set => SetProperty( ref _useImperial, value );
    }

    public bool UseCompassHeadings
    {
        get => _useCompass;
        set=> SetProperty( ref _useCompass, value);
    }

    public string? DefaultCallback
    {
        get => _defaultCallback;
        set => SetProperty( ref _defaultCallback, value );
    }

    public int DefaultDaysBack
    {
        get => _defaultDaysBack;
        set => SetProperty( ref _defaultDaysBack, value );
    }

    public int MaxSmsLength { get; private set; } = 160;
}