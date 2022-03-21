using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.UI.Dispatching;

namespace J4JSoftware.GPSLocator;

public partial class StatusMessages
{
    public event EventHandler<StatusMessage>? DisplayMessage;

    private readonly DispatcherQueue _dQueue;
    private readonly List<StatusMessage> _queued = new();

    public StatusMessages(
        DispatcherQueue dQueue
    )
    {
        _dQueue = dQueue;
    }

    public StatusMessage Message(string text)
    {
        return new StatusMessage(text, this);
    }

    public StatusMessage QueueReady()
    {
        return Message( "Ready" );
    }

    public void DisplayReady()
    {
        _queued.Add( new StatusMessage( "Ready", this ) );

        Display();
    }

    public void Display()
    {
        foreach( var msg in _queued )
        {
            _dQueue.TryEnqueue( () =>
            {
                DisplayMessage?.Invoke( this, msg );
                Thread.Sleep(msg.Pause);
            });

        }

        _queued.Clear();
    }
}
