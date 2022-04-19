using System;
using System.Collections.Generic;

namespace J4JSoftware.GPSCommon;

public record AnnotatedMapType( MapType MapType )
{
    public static List<AnnotatedMapType> Choices { get; }

    static AnnotatedMapType()
    {
        Choices = new List<AnnotatedMapType>();

        foreach( var mapType in Enum.GetValues<MapType>() )
        {
            Choices.Add( new AnnotatedMapType( mapType ) );
        }
    }

    public string Label => MapType.GetDescription();
}
