using System.ComponentModel;
using J4JSoftware.DependencyInjection;

namespace J4JSoftware.GPSLocator;

public interface IDeviceConfig : INotifyPropertyChanged
{
    public event EventHandler<ValidationPhase>? Validation;

    public void Initialize(IDeviceContext context);

    public string Website { get; set; }
    public string IMEI { get; set; }
    public string UserName { get; set; }
    public string? Password { get; set; }

    public EncryptedString EncryptedPassword { get; }

    public ValidationState ValidationState { get; set; }
    public bool IsValid { get; }
    public Task<bool> ValidateAsync();
}
