using System;
using System.Text;
using System.Text.Json.Serialization;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

public class InReachConfig
{
    private string _website = string.Empty;
    private string _imei = string.Empty;
    private string _userName = string.Empty;
    private IJ4JLogger? _logger;

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

            _website = value;

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

            _imei = value;

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

            _userName = value;

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

    public EncryptedString EncryptedPassword { get; } = new();

    [JsonIgnore]
    public ValidationState ValidationState { get; private set; }
    
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
}
