using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public class HistoryViewModel : SelectablePointViewModel
{
    private bool _mustHaveMsg;
    private bool _hideInvalidLoc;

    public HistoryViewModel(
        RetrievedPoints<AppConfig> displayedPoints,
        AppViewModel appViewModel,
        CachedLocations cachedLocations,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base(displayedPoints, appViewModel, cachedLocations, statusMessages, logger)
    {
        RetrievedPoints.MapPointsFilter = new HistoryMapPointsFilter
        {
            HideInvalid = AppViewModel.Configuration.HideInvalidLocations,
            RequireMessage = MustHaveMessages
        };

        _hideInvalidLoc = AppViewModel.Configuration.HideInvalidLocations;
    }

    public bool HideInvalidLocations
    {
        get => _hideInvalidLoc;

        set
        {
            var changed = _hideInvalidLoc != value;

            SetProperty( ref _hideInvalidLoc, value );

            if( !changed )
                return;

            if( RetrievedPoints.MapPointsFilter is not HistoryMapPointsFilter filter )
                return;

            filter.HideInvalid = _hideInvalidLoc;
        }
    }

    public bool MustHaveMessages
    {
        get => _mustHaveMsg;

        set
        {
            var changed = _mustHaveMsg != value;

            SetProperty( ref _mustHaveMsg, value );

            if (!changed)
                return;

            if (RetrievedPoints.MapPointsFilter is not HistoryMapPointsFilter filter)
                return;

            filter.RequireMessage = _mustHaveMsg;
        }
    }
}