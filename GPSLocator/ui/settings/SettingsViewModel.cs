using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Text.Json.Serialization;
using J4JSoftware.GPSCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Serilog.Events;

namespace J4JSoftware.GPSLocator;

public class SettingsViewModel : ObservableObject
{
    private readonly AppViewModel _appViewModel;
    private readonly string _userConfigPath;
    private readonly IJ4JLogger _logger;
    private readonly IJ4JProtection _protector;
    private readonly DispatcherQueue _dQueue;
    private readonly StatusMessage.StatusMessages _statusMessages;

    private string _website = string.Empty;
    private string _userName = string.Empty;
    private string? _password;
    private string _imei = string.Empty;
    private bool _compassHeadings;
    private bool _imperialUnits;
    private bool _hideInvalidLoc;
    private LogEventLevel _minEventLevel;
    private string? _defaultCallback;
    private int _defaultDaysBack;
    private bool _validCallback;
    private SelectablePage? _launchPage;
    private bool _validated;
    private bool _deviceConfigChanged;
    private bool _otherConfigChanged;

    public SettingsViewModel(
        AppViewModel appViewModel,
        StatusMessage.StatusMessages statusMessages,
        IJ4JProtection protector,
        IJ4JHost host,
        IJ4JLogger logger
    )
    {
        _appViewModel = appViewModel;
        _statusMessages = statusMessages;
        _userConfigPath = host.UserConfigurationFiles.First();
        _protector = protector;

        _logger = logger;
        _logger.SetLoggedType( GetType() );

        _dQueue = DispatcherQueue.GetForCurrentThread();

        SaveCommand = new AsyncRelayCommand( SaveHandlerAsync );
        ValidateCommand = new AsyncRelayCommand( ValidateHandlerAsync );
        RevertCommand = new RelayCommand( RevertHandler );

        LogLevels = Enum.GetValues<LogEventLevel>().ToList();
    }

    public void OnLoaded()
    {
        RevertHandler();

        Validated = _appViewModel.Configuration.IsValid;
        DeviceConfigChanged = false;
        OtherConfigChanged = false;
    }

    public AsyncRelayCommand SaveCommand { get; }

    private async Task SaveHandlerAsync()
    {
        if( !Validated )
            return;

        _statusMessages.Message( "Saving configuration" ).Enqueue( 500 );

        UpdateAppConfig();

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        jsonOptions.Converters.Add( new JsonStringEnumConverter() );

        // create a temporary object to write because we don't want to include
        // appConfig stuff in the userConfig file
        var tempConfig = new
        {
            _appViewModel.Configuration.Website,
            _appViewModel.Configuration.UserName,
            _appViewModel.Configuration.EncryptedPassword,
            _appViewModel.Configuration.IMEI,
            _appViewModel.Configuration.UseCompassHeadings,
            _appViewModel.Configuration.UseImperialUnits,
            _appViewModel.Configuration.HideInvalidLocations,
            _appViewModel.Configuration.MinimumLogLevel,
            _appViewModel.Configuration.DefaultCallback,
            _appViewModel.Configuration.DefaultDaysBack,
            _appViewModel.Configuration.LaunchPage,
        };

        var text = JsonSerializer.Serialize( tempConfig, jsonOptions );
        var dirPath = Path.GetDirectoryName( _userConfigPath );

        Directory.CreateDirectory( dirPath! );
        await File.WriteAllTextAsync( _userConfigPath, text );

        _statusMessages.Message( "Configuration saved" ).Enqueue( 500 );
        _statusMessages.DisplayReady();

        DeviceConfigChanged = false;
        OtherConfigChanged = false;
    }

    private void UpdateAppConfig()
    {
        _appViewModel.Configuration.Website = Website;
        _appViewModel.Configuration.UserName = UserName;
        _appViewModel.Configuration.Password = Password;
        _appViewModel.Configuration.IMEI = Imei;
        _appViewModel.Configuration.UseCompassHeadings = CompassHeadings;
        _appViewModel.Configuration.UseImperialUnits = ImperialUnits;
        _appViewModel.Configuration.HideInvalidLocations = HideInvalidLocations;
        _appViewModel.Configuration.MinimumLogLevel = MinimumLogLevel;
        _appViewModel.Configuration.DefaultCallback = DefaultCallback;
        _appViewModel.Configuration.DefaultDaysBack = DefaultDaysBack;
        _appViewModel.Configuration.LaunchPage = LaunchPage?.PageTag;
    }

    public AsyncRelayCommand ValidateCommand { get; }

    private async Task ValidateHandlerAsync()
    {
        _statusMessages.Message( "Validating configuration" ).Display( 500 );

        // test the proposed configuration
        var testConfig = new DeviceConfig()
        {
            IMEI = Imei, UserName = UserName, Website = Website, Password = Password
        };

        testConfig.Validation += OnValidationProgress;

        testConfig.Initialize( new GpsLocatorContext( _protector, _logger ) );

        Validated = await testConfig.ValidateAsync();

        if( Validated )
        {
            UpdateAppConfig();
            _appViewModel.Configuration.ValidationState = ValidationState.Validated;

            _statusMessages.Message( "Validation succeeded").Important().Enqueue( 500 );
            _statusMessages.DisplayReady();
        }
        else
        {
            var failure = ( testConfig.ValidationState & ValidationState.CredentialsValid )
             == ValidationState.CredentialsValid
                    ? "Invalid IMEI"
                    : "Invalid user name and/or password";

            _statusMessages.Message( "Validation failed" ).Urgent().Enqueue();
            _statusMessages.Message( failure ).Urgent().Enqueue();
            _statusMessages.DisplayReady();
        }
    }

    private void OnValidationProgress( object? sender, ValidationPhase args )
    {
        _dQueue.TryEnqueue(() =>
        {
            (string msg, bool pBar) = args switch
            {
                ValidationPhase.Starting => ("Validation starting", true),
                ValidationPhase.NotValidatable => ("Validation failed due to invalid initialization", false),
                ValidationPhase.CheckingCredentials => ("Checking credentials", true),
                ValidationPhase.CheckingImei=> ("Checking IMEI", true),
                ValidationPhase.Failed=>("Validation failed", false),
                ValidationPhase.Succeeded=>("Validation succeeded", false),
                _ => throw new InvalidEnumArgumentException($"Unsupported {typeof(ValidationPhase)} '{args}'")
            };

            if( pBar )
                _statusMessages.Message(msg).Indeterminate().Display();
            else _statusMessages.Message(msg).Display();
        });
    }

    public RelayCommand RevertCommand { get; }

    private void RevertHandler()
    {
        Website = _appViewModel.Configuration.Website;
        UserName = _appViewModel.Configuration.UserName;
        Password = _appViewModel.Configuration.Password;
        Imei = _appViewModel.Configuration.IMEI;
        Validated = _appViewModel.Configuration.IsValid;
        CompassHeadings = _appViewModel.Configuration.UseCompassHeadings;
        ImperialUnits = _appViewModel.Configuration.UseImperialUnits;
        HideInvalidLocations = _appViewModel.Configuration.HideInvalidLocations;
        MinimumLogLevel = _appViewModel.Configuration.MinimumLogLevel;
        DefaultCallback = _appViewModel.Configuration.DefaultCallback;
        DefaultDaysBack = _appViewModel.Configuration.DefaultDaysBack;
        LaunchPage = NavigationTargets.Pages
                                 .FirstOrDefault( x => x.PageTag.Equals( _appViewModel.Configuration.LaunchPage,
                                                                       StringComparison.OrdinalIgnoreCase ) );

        DeviceConfigChanged = true;
    }

    public string Website
    {
        get => _website;

        set
        {
            DeviceConfigChanged = !string.Equals( _website, value, StringComparison.OrdinalIgnoreCase );

            SetProperty( ref _website, value );

            Validated = false;
        }
    }

    public string UserName
    {
        get => _userName;

        set
        {
            DeviceConfigChanged = !string.Equals(_website, value, StringComparison.OrdinalIgnoreCase);

            SetProperty( ref _userName, value );

            Validated = false;
        }
    }

    public string? Password
    {
        get => _password;

        set
        {
            DeviceConfigChanged = !string.Equals(_website, value, StringComparison.OrdinalIgnoreCase);

            SetProperty( ref _password, value );

            Validated = false;
        }
    }

    public string Imei
    {
        get => _imei;
            
        set
        {
            DeviceConfigChanged = !string.Equals(_website, value, StringComparison.OrdinalIgnoreCase);

            SetProperty( ref _imei, value );
                
            Validated = false;
        }
    }

    public bool CompassHeadings
    {
        get => _compassHeadings;

        set
        {
            SetProperty( ref _compassHeadings, value );
            OtherConfigChanged = true;
        }
    }

    public bool ImperialUnits
    {
        get => _imperialUnits;

        set
        {
            SetProperty( ref _imperialUnits, value );
            OtherConfigChanged = true;
        }
    }

    public bool HideInvalidLocations
    {
        get => _hideInvalidLoc;

        set
        {
            SetProperty( ref _hideInvalidLoc, value );
            OtherConfigChanged = true;
        }
    }

    public List<LogEventLevel> LogLevels { get; }

    public LogEventLevel MinimumLogLevel
    {
        get => _minEventLevel;

        set
        {
            SetProperty( ref _minEventLevel, value );
            OtherConfigChanged = true;
        }
    }

    public string? DefaultCallback
    {
        get => _defaultCallback;

        set
        {
            SetProperty( ref _defaultCallback, value );
            OtherConfigChanged = true;

            CallbackIsValid = ValidateCallback();
        }
    }

    public bool CallbackIsValid
    {
        get => _validCallback;

        set
        {
            SetProperty( ref _validCallback, value );
            OnPropertyChanged( nameof( CanSave ) );
        }
    }

    private bool ValidateCallback()
    {
        if( string.IsNullOrEmpty( DefaultCallback ) )
            return true;

        if( MailAddress.TryCreate( DefaultCallback, out _ ) )
            return true;

        if( DefaultCallback.All( char.IsDigit ) )
            return DefaultCallback.Length >= 10;

        return false;
    }

    public int DefaultDaysBack
    {
        get => _defaultDaysBack;

        set
        {
            SetProperty( ref _defaultDaysBack, value );
            OtherConfigChanged = true;
        }
    }

    public SelectablePage? LaunchPage
    {
        get => _launchPage;

        set
        {
            if ( value != null && value.PageTag.Equals( ResourceNames.NullPageName, StringComparison.OrdinalIgnoreCase ) )
                value = null;

            OtherConfigChanged = true;

            SetProperty( ref _launchPage, value );
        }
    }

    public bool Validated
    {
        get => _validated;

        set
        {
            SetProperty( ref _validated, value );
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public bool DeviceConfigChanged
    {
        get => _deviceConfigChanged;

        set
        {
            SetProperty( ref _deviceConfigChanged, value );
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public bool OtherConfigChanged
    {
        get => _otherConfigChanged;

        set
        {
            SetProperty( ref _otherConfigChanged, value );
            OnPropertyChanged( nameof( CanSave ) );
        }
    }

    public bool CanSave => Validated && CallbackIsValid && (DeviceConfigChanged || OtherConfigChanged);
}