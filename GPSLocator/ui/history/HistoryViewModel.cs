using System;
using System.Linq;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public class HistoryViewModel : SelectablePointViewModel<AppConfig>
{
    private bool _initialized;
    private bool _mustHaveMessages;
    private bool _hideInvalidLoc;

    public HistoryViewModel(
        AppViewModel appViewModel,
        CachedLocations cachedLocations,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base(appViewModel, cachedLocations, statusMessages, logger)
    {
    }

    public void OnPageActivated()
    {
        if (_initialized)
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

    public bool MustHaveMessages
    {
        get => _mustHaveMessages;

        set
        {
            var changed = value != _mustHaveMessages;

            SetProperty( ref _mustHaveMessages, value );

            if( !changed || !_mustHaveMessages )
                return;

            UpdateLocations();
        }
    }

    protected override bool IncludeLocation( ILocation location )
    {
        if( !HideInvalidLocations )
            return !MustHaveMessages || ( MustHaveMessages && location.HasMessage );

        if( !location.Coordinate.IsValid )
            return false;

        return !MustHaveMessages || (MustHaveMessages && location.HasMessage);
    }
}