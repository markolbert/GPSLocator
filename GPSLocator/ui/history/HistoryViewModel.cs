using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public class HistoryViewModel : SelectablePointViewModel
{
    private bool _mustHaveMessages;

    public HistoryViewModel(
        AppViewModel appViewModel,
        IJ4JLogger logger
    )
        : base(appViewModel, logger)
    {
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

            var locations = AllPoints.Where( x => x.DeviceLocation.HasMessage )
                                     .Select( x => x.DeviceLocation )
                                     .ToList();

            AddLocations( locations );
        }
    }

    protected override bool LocationFilter(Location toCheck) => toCheck.HasMessage || !_mustHaveMessages;
}