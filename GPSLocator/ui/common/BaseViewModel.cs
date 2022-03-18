using System;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace J4JSoftware.GPSLocator;

public class BaseViewModel : ObservableRecipient
{
    private readonly DispatcherQueue _dQueue;

    private Action? _onRequestStarted;
    private Action? _onRequestEnded;

    protected BaseViewModel(
        IJ4JLogger logger
    )
    {
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

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        Messenger.UnregisterAll(this);
    }

    protected virtual async Task<DeviceResponse<TResponse>> ExecuteRequestAsync<TResponse>(
        DeviceRequestBase<TResponse> request,
        Action? onRequestStarted,
        Action? onRequestEnded
    )
        where TResponse : class, new()
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
