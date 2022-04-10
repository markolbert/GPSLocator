using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

public class DeviceConfig : INotifyPropertyChanged
{
    public event EventHandler<ValidationPhase>? Validation;
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _website = string.Empty;
    private string _imei = string.Empty;
    private string _userName = string.Empty;
    private ValidationState _validationState = ValidationState.Unvalidated;
    private IJ4JLogger? _logger;
    private IBullshitLogger? _bsLogger;

    public DeviceConfig()
    {
        EncryptedPassword = new EncryptedString();
        EncryptedPassword.PropertyChanged += EncryptedPasswordOnPropertyChanged;
    }

    private void EncryptedPasswordOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        OnPropertyChanged( nameof( EncryptedPassword ) );
    }

    public virtual void Initialize( IDeviceContext context )
    {
        EncryptedPassword.Protector = context.Protector;
        EncryptedPassword.Logger = context.Logger;

        _logger = context.Logger;
        _bsLogger = context.BSLogger;
    }

    public string Website
    {
        get => _website;

        set
        {
            var changed = !string.Equals( _website, value, StringComparison.OrdinalIgnoreCase );
            SetProperty( ref _website, value );

            if( changed )
                ValidationState = ValidationState.Unvalidated;
        }
    }

    public string IMEI
    {
        get => _imei;

        set
        {
            var changed = !string.Equals(_imei, value, StringComparison.OrdinalIgnoreCase);
            SetProperty( ref _imei, value );

            if( changed )
                ValidationState &= ~ValidationState.ImeiValid;
        }
    }

    public string UserName
    {
        get => _userName;

        set
        {
            var changed = !string.Equals(_userName, value, StringComparison.OrdinalIgnoreCase);
            SetProperty( ref _userName, value );

            if (changed)
                ValidationState = ValidationState.Unvalidated;
        }
    }

    [JsonIgnore]
    public string? Password
    {
        get => EncryptedPassword.ClearText;

        set
        {
            var changed = !string.Equals(EncryptedPassword.ClearText, value, StringComparison.OrdinalIgnoreCase);

            EncryptedPassword.ClearText = value;

            if (changed)
                ValidationState = ValidationState.Unvalidated;
        }
    }

    public EncryptedString EncryptedPassword { get; }

    [ JsonIgnore ]
    public ValidationState ValidationState
    {
        get => _validationState;

        set
        {
            var changed = _validationState != value;
            SetProperty( ref _validationState, value );

            if( changed )
                OnPropertyChanged( nameof( IsValid ) );
        }
    }

    [JsonIgnore]
    public bool IsValid => ( ValidationState & ValidationState.Validated ) == ValidationState.Validated;

    public async Task<bool> ValidateAsync()
    {
        if( ( ValidationState & ValidationState.Validated ) == ValidationState.Validated )
        {
            Validation?.Invoke( this, ValidationPhase.Succeeded );
            return true;
        }

        if( _logger == null || EncryptedPassword.Protector == null )
        {
            _logger?.Error( "Configuration not initialized" );
            _bsLogger?.Log( "Configuration not initialized" );

            Validation?.Invoke( this, ValidationPhase.NotValidatable );
            return false;
        }

        _bsLogger?.Log( "Configuration initialized" );

        Validation?.Invoke( this, ValidationPhase.Starting );

        var isValid = await ValidateCredentialsAsync();
        isValid &= await ValidateImeiAsync();

        Validation?.Invoke( this, isValid ? ValidationPhase.Succeeded : ValidationPhase.Failed );

        return isValid;
    }

    private async Task<bool> ValidateCredentialsAsync()
    {
        Validation?.Invoke(this, ValidationPhase.CheckingCredentials);
        _bsLogger?.Log("Beginning credential validation");

        if( ( ValidationState & ValidationState.CredentialsValid ) == ValidationState.CredentialsValid )
        {
            _bsLogger?.Log("Credentials already valid");
            return true;
        }

        var testReq = new DeviceConfigRequest(this, _logger!, _bsLogger!);

        DeviceResponse<DeviceParameters>? response = null;

        try
        {
            response = await testReq.ExecuteAsync();
            _bsLogger?.Log("Returned from ExecuteAsync()");
        }
        catch ( Exception ex )
        {
            _bsLogger?.Log( $"Exception return from ExecuteAsync(), message was '{ex.Message}', exception was {ex.GetType().Name}" );
            return false;
        }

        if ( response!.Succeeded )
        {
            _bsLogger?.Log( "Credentials are valid" );
            ValidationState |= ValidationState.CredentialsValid;
        }
        else _bsLogger?.Log( "Credentials are invalid" );

        return response.Succeeded;
    }

    private async Task<bool> ValidateImeiAsync()
    {
        Validation?.Invoke(this, ValidationPhase.CheckingImei);
        _bsLogger?.Log("Beginning IMEI validation");

        if( ( ValidationState & ValidationState.Validated ) == ValidationState.Validated )
        {
            _bsLogger?.Log("IMEI already validated");
            return true;
        }

        var testReq = new LastKnownLocationRequest<Location>( this, _logger!, _bsLogger! );
        var response = await testReq.ExecuteAsync();

        var retVal = response.Succeeded && response.Result!.Locations.Count > 0;
        if( retVal )
        {
            _bsLogger?.Log( "IMEI is valid" );
            ValidationState |= ValidationState.ImeiValid;
        }
        else _bsLogger?.Log( "IMEI is invalid" );

        return retVal;
    }

    protected virtual void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null ) =>
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

    protected void SetProperty<T>( ref T field, T value, [ CallerMemberName ] string? propertyName = null )
    {
        field = value;
        OnPropertyChanged( propertyName );
    }
}
