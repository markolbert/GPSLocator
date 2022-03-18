using System.Text.Json;
using System.Text.Json.Serialization;
using J4JSoftware.GPSLocator.Annotations;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

[InboundV1("Messaging.svc","Message", true)]
public class SendMessageRequest : DevicePostRequest<SendMessageCount, object>
{
    public class MessageCollection
    {
        public MessageCollection( SendMessageRequest mesgReq )
        {
            Messages = new[] { new MessageData(mesgReq) };
        }

        public MessageData[] Messages { get; }
    }

    public class MessageData
    {
        public MessageData(
            SendMessageRequest mesgReq
        )
        {
            Sender = mesgReq.Sender;
            Timestamp = DateTime.UtcNow;
            Message = mesgReq.Text;
            Recipients = new[] { mesgReq.Configuration.IMEI };
        }

        [JsonConverter(typeof(Base64EncodedStringConverter))]
        public string Sender { get; }

        [JsonConverter(typeof(Iso8601DateTimeConverter))]
        public DateTime Timestamp { get; }

        [JsonConverter(typeof(Base64EncodedStringConverter))]
        public string Message { get; }
        public string[] Recipients { get; }
    }

    public SendMessageRequest( 
        DeviceConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }

    public string Text { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;

    protected override object GetJsonPostObject()
    {
        var msSince1970 = new DateTimeOffset( DateTime.UtcNow );
        var timestamp = $"/Date({msSince1970.ToUnixTimeMilliseconds()})/";

        var recipients = new string[] { Configuration.IMEI };

        var mesg = new { Sender = this.Sender, Message = this.Text, Recipients = recipients, Timestamp = timestamp };

        var messages = new { Messages = new[] { mesg } };

        return messages;
    }
}
