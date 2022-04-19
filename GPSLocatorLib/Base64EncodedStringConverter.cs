using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.GPSLocator
{
    public class Base64EncodedStringConverter : JsonConverter<string>
    {
        public override string Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options ) =>
            Encoding.UTF8.GetString( reader.GetBytesFromBase64() );

        public override void Write( Utf8JsonWriter writer, string value, JsonSerializerOptions options ) =>
            writer.WriteBase64StringValue( Encoding.UTF8.GetBytes( value ) );
    }
}
