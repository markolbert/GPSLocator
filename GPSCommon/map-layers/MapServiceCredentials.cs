using System.Text.Json.Serialization;
using J4JSoftware.DependencyInjection;

namespace J4JSoftware.GPSCommon;

public class MapServiceCredentials
{
    public MapServiceType ServiceType { get; set; }

    [JsonIgnore]
    public string? ApiKey
    {
        get => EncryptedApiKey.ClearText;
        set => EncryptedApiKey.ClearText = value;
    }

    public EncryptedString EncryptedApiKey { get; } = new();
}