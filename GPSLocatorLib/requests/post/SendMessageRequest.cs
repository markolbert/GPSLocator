using System.Text.Json;
using System.Text.Json.Serialization;
using J4JSoftware.GPSLocator.Annotations;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

[InboundV1("Messaging.svc","Message", true)]
public class SendMessageRequest : DevicePostRequest<SendMessageCount, GarminErrorMultiImei>
{
    public record MessageInfo( string Callback, string Text );

    private readonly List<MessageInfo> _messages = new();

    public SendMessageRequest( 
        DeviceConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }

    public void AddMessage( MessageInfo msgInfo )
    {
        _messages.Add( msgInfo );
    }

    public void AddMessage( string callback, string text )
    {
        AddMessage( new MessageInfo( callback, text ) );
    }

    protected override object GetJsonPostObject()
    {
        var msSince1970 = new DateTimeOffset( DateTime.UtcNow );
        var timestamp = $"/Date({msSince1970.ToUnixTimeMilliseconds()})/";

        var messages = new object[ _messages.Count ];
        var recipients = new[] { Configuration.IMEI };

        for (var idx = 0; idx < _messages.Count; idx++ )
        {
            messages[ idx ] = new
            {
                Sender = _messages[ idx ].Callback,
                Message = _messages[ idx ].Text,
                Recipients = recipients,
                Timestamp = timestamp
            };
        }

        return new { Messages = messages };
    }
}
