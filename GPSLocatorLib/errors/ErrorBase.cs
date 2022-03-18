using System.Net;
using System.Text;

namespace J4JSoftware.GPSLocator;

public abstract class ErrorBase
{
    protected ErrorBase()
    {
    }

    public GarminErrorCodes Code { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public HttpStatusCode HttpResponseCode { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if( HttpResponseCode != 0)
            sb.AppendText($"HttpResponseCode: {HttpResponseCode}");

        if( Code != 0 )
            sb.AppendText( $"ErrorCode: {Code}" );

        var imeiText = GetImeiAsText();
        if( !string.IsNullOrEmpty( imeiText ) )
            sb.AppendText( $"IMEI: {imeiText}" );

        if( !string.IsNullOrEmpty( Message ) )
            sb.AppendText( $"Message: {Message}" );

        if( !string.IsNullOrEmpty( Description ) )
            sb.AppendText( $"Description: {Description}" );

        return sb.ToString();
    }

    protected abstract string? GetImeiAsText();
}