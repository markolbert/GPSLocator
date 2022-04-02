using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Dispatching;

namespace J4JSoftware.GPSLocator;

public class LaunchViewModel : ObservableObject
{
    public event EventHandler? Initialized;

    private readonly IJ4JLogger _logger;
    private readonly AppViewModel _appViewModel;
    private readonly DispatcherQueue _dQueue;

    private string? _mesg;
    private Color _mesgColor = Colors.Gold;

    public LaunchViewModel( 
        AppViewModel appViewModel,
        IJ4JLogger logger 
        )
    {
        _dQueue = DispatcherQueue.GetForCurrentThread();

        _logger = logger;
        _logger.SetLoggedType( GetType() );

        _appViewModel = appViewModel;
    }

    public void OnPageActivated()
    {
        _appViewModel.Configuration.Validation += OnValidationProgress;

        Task.Run( async () => await _appViewModel.Configuration.ValidateAsync() );
    }

    private void OnValidationProgress( object? sender, ValidationPhase args )
    {
        OnValidationUpdate( args );

        if( args is not (ValidationPhase.Failed or ValidationPhase.Succeeded) )
            return;

        Thread.Sleep( 1000 ); // so last message will be visible
        Initialized?.Invoke( this, EventArgs.Empty );
    }

    private void OnValidationUpdate( ValidationPhase args )
    {
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

            if( isError )
                _logger.Error<string>( "Validating credentials: {0}", msg );
            else
                _logger.Information<string>( "Validating credentials: {0}", msg );

            if( terminate )
                _appViewModel.Configuration.Validation -= OnValidationProgress;
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