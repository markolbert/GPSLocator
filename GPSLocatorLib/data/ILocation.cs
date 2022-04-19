namespace J4JSoftware.GPSLocator;

public interface ILocation
{
    long IMEI { get; set; }
    DateTime Timestamp { get; set; }
    GeoLocation Coordinate { get; set; }
    double Altitude { get; set; }
    double Speed { get; set; }
    long Course { get; set; }
    int GPSFixStatus { get; set; }

    bool HasMessage { get; }
    string TextMessage { get; set; }
    List<string> Recipients { get; set; }
}
