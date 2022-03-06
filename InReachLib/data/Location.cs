using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using J4JSoftware.InReach.Annotations;

namespace J4JSoftware.InReach;

public class Location : ILocation
{
    public const double FeetPerMeter = 3.28084;
    public const double MPHperKMH = 0.621371;

    public event PropertyChangedEventHandler? PropertyChanged;

    private double _altitude;
    private double _speed;
    private bool _imperialUnits;

    public long IMEI { get; set; }

    [JsonConverter(typeof(InReachDateTimeConverter))]
    public DateTime Timestamp { get; set; }

    public GeoLocation Coordinate { get; set; } = new();

    public double Altitude
    {
        get => ImperialUnits ? Math.Round(FeetPerMeter * _altitude) : _altitude;

        set
        {
            _altitude = value;
            OnPropertyChanged();
        }
    }

    public double Speed
    {
        get => ImperialUnits ? Math.Round(MPHperKMH * _speed) : _speed;

        set
        {
            _speed = value;
            OnPropertyChanged();
        }
    }

    public long Course { get; set; }
    public int GPSFixStatus { get; set; }

    public bool ImperialUnits
    {
        get => _imperialUnits;

        set
        {
            _imperialUnits = value;
            OnPropertyChanged();

            OnPropertyChanged( nameof( Altitude ) );
            OnPropertyChanged( nameof( AltitudeUnits ) );
            OnPropertyChanged( nameof( Speed ) );
            OnPropertyChanged( nameof( SpeedUnits ) );
        }
    }
    public string AltitudeUnits => ImperialUnits ? "feet" : "meters";
    public string SpeedUnits => ImperialUnits ? "mph" : "km/h";

    [ NotifyPropertyChangedInvocator ]
    protected virtual void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }
}