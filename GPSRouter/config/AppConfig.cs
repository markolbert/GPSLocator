using System;
using System.Collections.Generic;
using System.Reflection;
using J4JSoftware.GPSCommon;
using Serilog.Events;

namespace J4JSoftware.GPSLocator;

public class AppConfig : BaseAppConfig
{
    private bool _useImperial;
    private bool _useCompass;
    private int _defaultDaysBack;

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

    public int DefaultDaysBack
    {
        get => _defaultDaysBack;
        set => SetProperty( ref _defaultDaysBack, value );
    }
}