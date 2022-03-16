using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using J4JSoftware.DependencyInjection;
using J4JSoftware.InReach.Annotations;
using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

public class InReachConfig : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _website = string.Empty;
    private string _imei = string.Empty;
    private string _userName = string.Empty;
    private ValidationState _validationState = ValidationState.Unvalidated;
    private IJ4JLogger? _logger;

    public InReachConfig()
    {
        EncryptedPassword = new EncryptedString();
        EncryptedPassword.PropertyChanged += EncryptedPasswordOnPropertyChanged;
    }

    private void EncryptedPasswordOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        OnPropertyChanged( nameof( EncryptedPassword ) );
    }

    public void Initialize( IJ4JProtection protector, IJ4JLogger logger )
    {
        EncryptedPassword.Protector = protector;
        EncryptedPassword.Logger = logger;

        _logger = logger;
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

    public async Task<bool> ValidateAsync( EventHandler startEvent, EventHandler endEvent )
    {
        if( ( ValidationState & ValidationState.Validated ) == ValidationState.Validated )
            return true;

        if( _logger == null || EncryptedPassword.Protector == null )
        {
            _logger?.Error( "Configuration not initialized" );
            return false;
        }

        if( ( ValidationState & ValidationState.CredentialsValid ) == ValidationState.CredentialsValid )
            return await ValidateImeiAsync( startEvent, endEvent );

        if( !await ValidateCredentialsAsync( startEvent, endEvent ) )
            return false;

        return await ValidateImeiAsync( startEvent, endEvent );
    }

    private async Task<bool> ValidateCredentialsAsync( EventHandler startEvent, EventHandler endEvent )
    {
        var testReq = new DeviceConfigRequest(this, _logger!);
        testReq.Started += startEvent;
        testReq.Ended += endEvent;

        var result = await testReq.ExecuteAsync();

        testReq.Started -= startEvent;
        testReq.Ended -= endEvent;

        if ( result.Succeeded )
            ValidationState |= ValidationState.CredentialsValid;

        return result.Succeeded;
    }

    private async Task<bool> ValidateImeiAsync(EventHandler startEvent, EventHandler endEvent)
    {
        var testReq = new LastKnownLocationRequest<Location>( this, _logger! );
        testReq.Started += startEvent;
        testReq.Ended += endEvent;

        var response = await testReq.ExecuteAsync();

        testReq.Started -= startEvent;
        testReq.Ended -= endEvent;

        if ( !response.Succeeded || response.Result!.Locations.Count <= 0 )
            return false;

        ValidationState |= ValidationState.ImeiValid;

        return true;
    }

    [ NotifyPropertyChangedInvocator ]
    protected virtual void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null ) =>
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

    protected void SetProperty<T>( ref T field, T value, [ CallerMemberName ] string? propertyName = null )
    {
        field = value;
        OnPropertyChanged( propertyName );
    }
}
