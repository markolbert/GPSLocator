using System;
using System.Linq;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public class HistoryViewModel : SelectablePointViewModel<AppConfig>
{
    private bool _mustHaveMessages;
    private bool _hideInvalidLoc;

    public HistoryViewModel(
        DisplayedPoints displayedPoints,
        AppViewModel appViewModel,
        CachedLocations cachedLocations,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base(displayedPoints, appViewModel, cachedLocations, statusMessages, logger)
    {
        HideInvalidLocations = AppViewModel.Configuration.HideInvalidLocations;
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

            if( !changed  )
                return;

            UpdateLocations();
        }
    }

    protected override bool IncludeLocation( MapPoint mapPoint )
    {
        if( !HideInvalidLocations )
            return !MustHaveMessages || ( MustHaveMessages && mapPoint.HasMessage );

        if( !mapPoint.IsValidLocation )
            return false;

        return !MustHaveMessages || (MustHaveMessages && mapPoint.HasMessage);
    }
}