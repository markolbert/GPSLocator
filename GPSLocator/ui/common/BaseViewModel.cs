using System;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Dispatching;

namespace J4JSoftware.GPSLocator;

public class BaseViewModel : ObservableValidator
{
    private readonly DispatcherQueue _dQueue;

    private Action? _onRequestStarted;
    private Action? _onRequestEnded;
    private bool _isActive;

    protected BaseViewModel(
        IJ4JLogger logger
    )
        : this( WeakReferenceMessenger.Default, logger )
    {
    }

    protected BaseViewModel(
        IMessenger messenger,
        IJ4JLogger logger
    )
    {
        Messenger = messenger;

        _dQueue = DispatcherQueue.GetForCurrentThread();

        IsActive = true;

        AppViewModel = (App.Current.Resources["AppViewModel"] as AppViewModel)!;

        Logger = logger;
        Logger.SetLoggedType( GetType() );

        if( AppViewModel.Configuration.IsValid )
            AppViewModel.SetStatusMessage("Ready" );
        else AppViewModel.SetStatusMessage("Invalid configuration", StatusMessageType.Important );
    }

    protected IJ4JLogger Logger { get; }
    public AppViewModel AppViewModel { get; }

    #region stuff to mimic ObservableRecipient

    protected IMessenger Messenger { get; }

    public bool IsActive
    {
        get => _isActive;

        set
        {
            if (SetProperty(ref _isActive, value, true))
            {
                if (value)
                    OnActivated();
                else OnDeactivated();
            }
        }
    }

    protected virtual void OnActivated()
    {
        Messenger.RegisterAll(this);
    }

    protected virtual void OnDeactivated()
    {
        Messenger.UnregisterAll(this);

        Messenger.UnregisterAll(this);
    }

    protected virtual void Broadcast<T>(T oldValue, T newValue, string? propertyName)
    {
        PropertyChangedMessage<T> message = new(this, propertyName, oldValue, newValue);

        _ = Messenger.Send(message);
    }

    #endregion

    protected virtual async Task<DeviceResponse<TResponse>> ExecuteRequestAsync<TResponse, TError>(
        DeviceRequestBase<TResponse, TError> request,
        Action? onRequestStarted,
        Action? onRequestEnded
    )
        where TResponse : class, new()
        where TError : ErrorBase, new()
    {
        _onRequestStarted = onRequestStarted;
        _onRequestEnded = onRequestEnded;

        request.Started += RequestStarted;
        request.Ended += RequestEnded;

        DeviceResponse<TResponse>? response = null;

        await Task.Run(async () =>
        {
            response = await request.ExecuteAsync();

            request.Started -= RequestStarted;
            request.Ended -= RequestEnded;
        });

        return response!;
    }

    private void RequestStarted(object? sender, EventArgs e)
    {
        if( _onRequestStarted != null )
            _dQueue.TryEnqueue( () => _onRequestStarted() );
    }

    private void RequestEnded( object? sender, EventArgs e )
    {
        if( _onRequestEnded != null )
            _dQueue.TryEnqueue( () => _onRequestEnded() );
    }
}
