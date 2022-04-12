using System;
using System.Collections.Generic;
using System.ComponentModel;
using ABI.System;
using J4JSoftware.GPSLocator;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GPSCommon;

public class MapPoint : ObservableObject
{
    public const double FeetPerMeter = 3.28084;
    public const double MPHperKMH = 0.621371;

    private readonly long _course;
    private readonly double _altitude;
    private readonly double _speed;

    private bool _imperialUnits;
    private bool _compassHeadings;

    public MapPoint(
        ILocation location
    )
        : this( location.Coordinate.Latitude, location.Coordinate.Longitude, location.Timestamp )
    {
        TextMessage = location.TextMessage;
        Recipients = location.Recipients;
        _altitude = location.Altitude;
        _speed = location.Speed;
        _course = location.Course >= 0 ? location.Course : location.Course + 360;
        GPSFixStatus = location.GPSFixStatus;
    }

    public MapPoint(
        double latitude,
        double longitude,
        DateTime timestamp
    )
    {
        Timestamp = timestamp;
        MapLocation = new MapControl.Location( latitude, longitude );
        Label =
            $"{latitude}, {longitude}\n{timestamp}";
    }

    public DateTime Timestamp { get; }
    public MapControl.Location MapLocation { get; }
    public string Label { get; }

    public string? TextMessage { get; }
    public List<string>? Recipients { get; }
    public bool HasMessage => !string.IsNullOrEmpty(TextMessage);

    public double Altitude => ImperialUnits ? Math.Round( FeetPerMeter * _altitude ) : _altitude;
    public string AltitudeUnits => ImperialUnits ? "feet" : "meters";

    public double Speed => ImperialUnits ? Math.Round(MPHperKMH * _speed) : _speed;
    public string SpeedUnits => ImperialUnits ? "mph" : "km/h";

    public string CourseDisplay
    {
        get
        {
            if (!_compassHeadings)
                return $"{_course:n0}";

            var cardinal = Convert.ToInt32(Math.Round(Convert.ToDouble(_course) / 22.5, 0));

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

    public string CompassUnits => CompassHeadings ? string.Empty : "degrees";

    public int GPSFixStatus { get; }

    public bool ImperialUnits
    {
        get => _imperialUnits;

        set
        {
            var changed = value != _imperialUnits;
            SetProperty( ref _imperialUnits, value );

            if( !changed )
                return;

            OnPropertyChanged( nameof( AltitudeUnits ) );
            OnPropertyChanged( nameof( Altitude ) );
            OnPropertyChanged(nameof(Speed));
            OnPropertyChanged( nameof( SpeedUnits ) );
        }
    }

    public bool CompassHeadings
    {
        get => _compassHeadings;

        set
        {
            var changed = value != _compassHeadings;

            SetProperty( ref _compassHeadings, value );

            if( !changed )
                return;

            OnPropertyChanged(nameof(CourseDisplay));
            OnPropertyChanged(nameof(CompassUnits));
        }
    }

}