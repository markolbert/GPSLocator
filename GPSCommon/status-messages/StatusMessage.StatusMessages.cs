using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.UI.Dispatching;

namespace J4JSoftware.GPSCommon;

public partial class StatusMessage
{
    public class StatusMessages
    {
        public const int DefaultMinimumPause = 250;

        public event EventHandler<StatusMessage>? DisplayMessage;

        private readonly DispatcherQueue _dQueue;
        private readonly ConcurrentQueue<StatusMessage> _queued = new();

        private Thread? _displayThread;

        public StatusMessages(
            DispatcherQueue dQueue,
            int globalMinPause = DefaultMinimumPause
        )
        {
            _dQueue = dQueue;

            if (globalMinPause < 0)
                globalMinPause = DefaultMinimumPause;

            GlobalMinimumPause = globalMinPause;
        }

        public int GlobalMinimumPause { get; }

        public void Add(StatusMessage msg) => _queued.Enqueue(msg);

        public StatusMessage Message(string text)
        {
            return new StatusMessage(text, this);
        }

        public StatusMessage EnqueueReady()
        {
            return Message("Ready");
        }

        public void DisplayReady()
        {
            _queued.Enqueue(new StatusMessage("Ready", this));
            Display();
        }

        public void Display()
        {
            if( ( _displayThread?.ThreadState ?? ThreadState.Stopped ) == ThreadState.Running )
                return;

            _displayThread = new Thread(DisplayMessages);
            _displayThread!.Start();
        }

        private void DisplayMessages()
        {
            while (_queued.TryDequeue(out var msg))
            {
                var copy = msg.Copy();

                // we have exclusive access to the message list
                _dQueue.TryEnqueue(() =>
                {
                    DisplayMessage?.Invoke(this, copy);
                });

                Thread.Sleep(copy.Pause.Maximum);
            }
        }

    }
}
