using System.Net;
using System.Text;

namespace J4JSoftware.InReach;

public class InReachError
{
    public int Code { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string IMEI { get; set; } = string.Empty;
    public HttpStatusCode HttpResponseCode { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if( HttpResponseCode != 0)
            sb.AppendText($"HttpResponseCode: {HttpResponseCode}");

        if( Code != 0 )
            sb.AppendText( $"ErrorCode: {Code}" );

        if( !string.IsNullOrEmpty( IMEI ) )
            sb.AppendText( $"IMEI: {IMEI}" );

        if( !string.IsNullOrEmpty( Message ) )
            sb.AppendText( $"Message: {Message}" );

        if( !string.IsNullOrEmpty( Description ) )
            sb.AppendText( $"Description: {Description}" );

        return sb.ToString();
    }
}
