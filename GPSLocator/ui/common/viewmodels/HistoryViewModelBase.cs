using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator;

public class HistoryViewModelBase : SelectablePointViewModel<AppConfig>
{
    private bool _initialized;
    private bool _hideInvalidLoc;

    protected HistoryViewModelBase(
        AppViewModel appViewModel,
        CachedLocations cachedLocations,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( appViewModel, cachedLocations, statusMessages, logger)
    {
        DaysBack = AppViewModel.Configuration.DefaultDaysBack;
        EndDate = DateTimeOffset.Now;
        HideInvalidLocations = AppViewModel.Configuration.HideInvalidLocations;
    }

    public void OnPageActivated()
    {
        if( _initialized )
            return;

        RefreshHandler();

        _initialized = true;
    }

    public bool HideInvalidLocations
    {
        get => _hideInvalidLoc;

        set
        {
            var changed = value != _hideInvalidLoc;

            SetProperty(ref _hideInvalidLoc, value);

            if (changed)
                UpdateLocations();
        }
    }
}