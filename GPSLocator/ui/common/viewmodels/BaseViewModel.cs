using System;
using System.Threading.Tasks;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Dispatching;

namespace J4JSoftware.GPSLocator;

public class BaseViewModel : ObservableValidator
{
    private readonly DispatcherQueue _dQueue;

    private object? _onStatusChanged;
    private bool _isActive;

    protected BaseViewModel(
        AppViewModel appViewModel,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : this( appViewModel, statusMessages, WeakReferenceMessenger.Default, logger )
    {
    }

    protected BaseViewModel(
        AppViewModel appViewModel,
        StatusMessage.StatusMessages statusMessages,
        IMessenger messenger,
        IJ4JLogger logger
    )
    {
        Messenger = messenger;

        _dQueue = DispatcherQueue.GetForCurrentThread();

        IsActive = true;

        AppViewModel = appViewModel;
        StatusMessages = statusMessages;

        Logger = logger;
        Logger.SetLoggedType( GetType() );

        if( !AppViewModel.Configuration.IsValid )
            StatusMessages.Message("Invalid configuration").Urgent().Enqueue();

        StatusMessages.DisplayReady();
    }

    protected IJ4JLogger Logger { get; }
    protected StatusMessage.StatusMessages StatusMessages { get; }

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

    protected virtual DeviceResponse<TResponse> ExecuteRequest<TResponse, TError>(
        DeviceRequestBase<TResponse, TError> request,
        Action<RequestEventArgs<TResponse>>? onStatusChanged
    )
        where TResponse : class, new()
        where TError : ErrorBase, new()
    {
        _onStatusChanged = onStatusChanged;
        request.Status += StatusChanged;

        DeviceResponse<TResponse>? response = null;

        Task.Run(async () =>
        {
            response = await request.ExecuteAsync();
            request.Status -= StatusChanged;
        });

        return response!;
    }

    private void StatusChanged<TResponse>(object? sender, RequestEventArgs<TResponse> args )
        where TResponse : class, new()
    {
        _dQueue.TryEnqueue( () =>
        {
            if( _onStatusChanged is Action<RequestEventArgs<TResponse>> statusChangeHandler )
                statusChangeHandler( args );
            else Logger.Error( "Expected a {0} but given a {1}",
                               typeof( Action<RequestEventArgs<TResponse>> ),
                               _onStatusChanged!.GetType() );
        } );
    }
}
