using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.GPSLocator
{
    // for handling dumb date formats like \/Date(1646336692443)\/
    public class Iso8601DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
        {
            var text = reader.GetString();
            if( text == null )
                throw new JsonException( "Could not retrieve DateTime text string" );

            text = text.Replace( "/", "" )
                       .Replace( "Date(", "" )
                       .Replace( ")", "" );

            if( !long.TryParse( text, out long msSince1970 ) )
                throw new JsonException( $"Could not parse datetime text '{text}'" );

            var retVal = DateTimeOffset.FromUnixTimeMilliseconds( msSince1970 ).LocalDateTime;

            return retVal;
        }

        public override void Write( Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options )
        {
            var msSince1970 = new DateTimeOffset( value );

            writer.WriteStringValue( $"/Date({msSince1970.ToUnixTimeMilliseconds()})/" );
        }
    }
}
