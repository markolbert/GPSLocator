using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.GPSLocator;

public class MessageQueue
{
    public static MessageQueue Default { get; } = new MessageQueue( DispatcherQueue.GetForCurrentThread() );

    public event EventHandler<StatusMessage>? DisplayMessage;

    public record MessageInfo(
        string Text,
        MessageLevel Level,
        int MsPause,
        ProgressBarType? ProgressBarType,
        int ProgressBarMax = 0
    );

    private readonly ConcurrentQueue<StatusMessage> _statusMessages = new();
    private readonly DispatcherQueue _dQueue;

    private bool _statusDisplayRunning;

    public MessageQueue(
        DispatcherQueue dQueue
    )
    {
        _dQueue = dQueue;
    }

    public StatusMessage Message( string text ) => new( text, this );
    public StatusMessage Ready() => Message( "Ready" );

    public void Add( StatusMessage msg )
    {
        _statusMessages.Enqueue( msg );

        if( _statusDisplayRunning )
            return;

        var displayThread = new Thread( new ThreadStart(DisplayStatusMessages) );
        displayThread.Start();
    }

    private void DisplayStatusMessages()
    {
        _statusDisplayRunning = true;

        Thread.Sleep( 100 );

        while (_statusMessages.Any())
        {
            if (!_statusMessages.TryDequeue(out var msg))
                continue;

            _dQueue.TryEnqueue(() =>
            {
                DisplayMessage?.Invoke(this, msg);
            });

            Thread.Sleep(msg.MsPause);
        }

        _statusDisplayRunning = false;
    }

}