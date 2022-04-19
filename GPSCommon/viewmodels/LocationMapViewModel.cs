using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using J4JSoftware.GPSLocator;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GPSCommon;

public class LocationMapViewModel<TAppConfig> : BaseViewModel
    where TAppConfig : BaseAppConfig
{
    protected LocationMapViewModel(
        MapViewModel retrievedPts,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( statusMessages, logger )
    {
        MapViewModel = retrievedPts;
    }

    public MapViewModel MapViewModel { get; }
}