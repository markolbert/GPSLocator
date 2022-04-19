using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Serilog.Events;

namespace J4JSoftware.GPSLocator;

public class LaunchViewModel : ObservableObject
{
    public event EventHandler? Initialized;

    private readonly IJ4JLogger _logger;
    private readonly IAppConfig _appConfig;
    private readonly DispatcherQueue _dQueue;

    private string? _mesg;
    private Color _mesgColor = Colors.Gold;

    public LaunchViewModel( 
        IAppConfig appConfig,
        IJ4JLogger logger 
        )
    {
        _dQueue = DispatcherQueue.GetForCurrentThread();

        _logger = logger;
        _logger.SetLoggedType( GetType() );

        _appConfig = appConfig;
    }

    public void OnPageActivated()
    {
        _logger.Information("Starting initial validation");
        _appConfig.Validation += OnValidationProgress;

        Task.Run( async () => await _appConfig.ValidateAsync() );

        _logger.Information<string>($"Exiting {0}", nameof(OnPageActivated));
    }

    private void OnValidationProgress( object? sender, ValidationPhase args )
    {
        _logger.Information("Validation event received {0}", args);

        OnValidationUpdate( args );

        if (args is not (ValidationPhase.Failed or ValidationPhase.Succeeded or ValidationPhase.NotValidatable))
            return;

        Thread.Sleep( 1000 ); // so last message will be visible

        _logger.Information<string>("Raising {0} event", nameof(Initialized));
        
        Initialized?.Invoke( this, EventArgs.Empty );
    }

    private void OnValidationUpdate( ValidationPhase args )
    {
        _logger.Information("Queuing validation update {0}", args);

        _dQueue.TryEnqueue( () =>
        {
            ( string msg, Color msgColor, bool isError, bool terminate ) = args switch
            {
                ValidationPhase.Starting => ( "Validation starting", Colors.Gold, false, false ),
                ValidationPhase.NotValidatable => ( "Invalid initialization", Colors.Red, true, true ),
                ValidationPhase.CheckingCredentials => ( "Checking credentials", Colors.Gold, false, false ),
                ValidationPhase.CheckingImei => ( "Checking IMEI", Colors.Gold, false, false ),
                ValidationPhase.Succeeded => ( "Validation succeeded", Colors.Gold, false, true ),
                ValidationPhase.Failed => ( "Validation failed", Colors.Red, true, true ),
                _ => throw new InvalidEnumArgumentException( $"Unsupported {typeof( ValidationPhase )} '{args}'" )
            };

            Message = msg;
            MessageColor = msgColor;

            _logger.Write<string>( isError ? LogEventLevel.Error : LogEventLevel.Information,
                                   "Updating user interface: {0}",
                                   msg );

            if( !terminate )
                return;

            _appConfig.Validation -= OnValidationProgress;
            _logger.Information( "Decoupled event handler" );
        } );
    }

    public string? Message
    {
        get => _mesg;
        set => SetProperty( ref _mesg, value );
    }

    public Color MessageColor
    {
        get => _mesgColor;
        set => SetProperty( ref _mesgColor, value );
    }
}