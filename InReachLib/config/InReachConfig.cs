using System;
using System.Text;
using System.Text.Json.Serialization;
using J4JSoftware.DependencyInjection;

namespace J4JSoftware.InReach;

public class InReachConfig : IInReachConfig
{
    public string Website { get; set; } = string.Empty;
    public string IMEI { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public EncryptedString Password { get; } = new();

    [ JsonIgnore ]
    public bool IsValid =>
        !string.IsNullOrEmpty( Website )
     && !string.IsNullOrEmpty( Website )
     && !string.IsNullOrEmpty( UserName )
     && !string.IsNullOrEmpty( Password.ClearText );

    [JsonIgnore]
    public string Credentials
    {
        get
        {
            var plainBytes = Encoding.UTF8.GetBytes( $"{UserName}:{Password}" );

            return $"Basic {Convert.ToBase64String( plainBytes )}";
        }
    }
}
