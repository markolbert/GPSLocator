using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using J4JSoftware.GPSLocator;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GPSCommon;

public class LocationMapViewModel<TAppConfig> : BaseViewModel<TAppConfig>
    where TAppConfig : BaseAppConfig
{
    protected LocationMapViewModel(
        RetrievedPoints retrievedPts,
        BaseAppViewModel<TAppConfig> appViewModel,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( appViewModel, statusMessages, logger )
    {
        RetrievedPoints = retrievedPts;
    }

    public RetrievedPoints RetrievedPoints { get; }
}