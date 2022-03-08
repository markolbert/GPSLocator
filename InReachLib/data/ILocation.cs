using System.ComponentModel;

namespace J4JSoftware.InReach;

public interface ILocation : INotifyPropertyChanged
{
    long IMEI { get; set; }
    DateTime Timestamp { get; set; }
    GeoLocation Coordinate { get; set; }
    double Altitude { get; set; }
    double Speed { get; set; }
    long Course { get; set; }
    int GPSFixStatus { get; set; }

    bool ImperialUnits { get; set; }
    public string AltitudeUnits { get; }
    public string SpeedUnits { get; }

    bool HasMessage { get; }
    string Message { get; set; }
    List<string> Recipients { get; set; }
}
