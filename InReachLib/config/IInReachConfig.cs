using J4JSoftware.DependencyInjection;

namespace J4JSoftware.InReach
{
    public interface IInReachConfig
    {
        string Website { get; set; }
        string IMEI { get; set; }
        string UserName { get; set; }
        EncryptedString Password { get; }
        string Credentials { get; }
        bool IsValid { get; }
    }
}
