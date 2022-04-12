using System;
using J4JSoftware.GPSLocator;

namespace J4JSoftware.GPSCommon;

public class CachedLocationEventArgs : EventArgs
{
    public CachedLocationEventArgs(
        RequestEvent phase
    )
    {
        Phase = phase;
    }

    public RequestEvent Phase { get; }
    public string? Message { get; set; }
}
