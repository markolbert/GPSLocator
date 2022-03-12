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
    private bool _imperialUnits = true;
    private bool _compassHeadings = true;

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

    public string CourseDisplay
    {
        get
        {
            if( !_compassHeadings )
                return $"{Course:n0}";

            var adjCourse = Course >= 0 ? Course : Course + 360;

            var cardinal = Convert.ToInt32( Math.Round( Convert.ToDouble( adjCourse ) / 22.5, 0 ) );

            return cardinal switch
            {
                0 => "N",
                1 => "NNE",
                2 => "NE",
                3 => "ENE",
                4 => "E",
                5 => "ESE",
                6 => "SE",
                7 => "SSE",
                8 => "S",
                9 => "SSW",
                10 => "SW",
                11 => "WSW",
                12 => "W",
                13 => "WNW",
                14 => "NW",
                15 => "NNW",
                _ => "Unknown"
            };
        }
    }

    public int GPSFixStatus { get; set; }

    public bool ImperialUnits
    {
        get => _imperialUnits;

        set
        {
            var changed = value != _imperialUnits;

            _imperialUnits = value;
            OnPropertyChanged();

            if( !changed )
                return;

            OnPropertyChanged( nameof( Altitude ) );
            OnPropertyChanged( nameof( AltitudeUnits ) );
            OnPropertyChanged( nameof( Speed ) );
            OnPropertyChanged( nameof( SpeedUnits ) );
        }
    }

    public bool CompassHeadings
    {
        get => _compassHeadings;

        set
        {
            var changed = value != _compassHeadings;

            _compassHeadings = value;
            OnPropertyChanged();

            if( changed )
            {
                OnPropertyChanged( nameof( CourseDisplay ) );
                OnPropertyChanged( nameof( CompassUnits ) );
            }
        }
    }

    public string AltitudeUnits => ImperialUnits ? "feet" : "meters";
    public string SpeedUnits => ImperialUnits ? "mph" : "km/h";
    public string CompassUnits => CompassHeadings ? string.Empty : "degrees";

    public bool HasMessage => !string.IsNullOrEmpty(Message);
    public string Message { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();

    [NotifyPropertyChangedInvocator ]
    protected virtual void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }
}