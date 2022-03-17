using System.Text;

namespace J4JSoftware.GPSLocator;

internal static class StringBuilderExtensions
{
    public static StringBuilder AppendText( this StringBuilder sb, string text )
    {
        if( sb.Length > 0 )
            sb.Append( ", " );

        sb.Append( text );

        return sb;
    }
}
