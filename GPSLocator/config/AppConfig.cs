using System;
using System.Reflection;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;
using Serilog.Events;

namespace J4JSoftware.GPSLocator;

public class AppConfig : BaseAppConfig
{
    private bool _useImperial;
    private bool _useCompass;
    private string? _defaultCallback;
    private int _defaultDaysBack;

    public AppConfig()
    {
        HelpLink = "https://www.jumpforjoysoftware.com/gpslocator-user-docs/";
    }

    public override void Initialize( string helpLink ) => Initialize(helpLink, 160, null);

    public void Initialize( string helpLink, int maxSmsLength, IJ4JLogger? logger )
    {
        if( logger != null )
            logger.SetLoggedType( GetType() );

        if (maxSmsLength <= 0)
        {
            logger?.Error("Invalid max SMS length ({0}), defaulting to 160 characters", maxSmsLength);
            maxSmsLength = 160;
        }

        base.Initialize(helpLink);

        MaxSmsLength = maxSmsLength;
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