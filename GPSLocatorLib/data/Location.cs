using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace J4JSoftware.GPSLocator;

public class Location : ILocation
{
    public long IMEI { get; set; }

    [JsonConverter(typeof(Iso8601DateTimeConverter))]
    public DateTime Timestamp { get; set; }

    public GeoLocation Coordinate { get; set; } = new();
    public double Altitude { get; set; }
    public double Speed { get; set; }
    public long Course { get; set; }
    public int GPSFixStatus { get; set; }
    public bool HasMessage => !string.IsNullOrEmpty(TextMessage);
    public string TextMessage { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
}