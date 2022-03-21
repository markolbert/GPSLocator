namespace J4JSoftware.GPSLocator;

public class StatusMessage
{
    private readonly MessageQueue _messageQueue;

    private ProgressBarType? _progressBarType;

    public StatusMessage(
        string text,
        MessageQueue queue
    )
    {
        Text = text;
        _messageQueue = queue;
    }

    public string Text { get; }

    public bool HasProgressBar => _progressBarType != null;

    public ProgressBarType? ProgressBarType
    {
        get => _progressBarType;
        private set => _progressBarType = value;
    }

    public int MaxProgressBar { get; private set; } = -1;

    public StatusMessage Indeterminate()
    {
        _progressBarType = GPSLocator.ProgressBarType.Indeterminate;
        return this;
    }

    public StatusMessage Determinate( int max )
    {
        _progressBarType = GPSLocator.ProgressBarType.Determinate;
        MaxProgressBar = max;

        return this;
    }

    public MessageLevel Importance { get; private set; } = MessageLevel.Normal;

    public StatusMessage Important() => Level( MessageLevel.Important );
    public StatusMessage Urgent() => Level( MessageLevel.Urgent );

    public StatusMessage Level( MessageLevel level )
    {
        Importance = level;
        return this;
    }

    public int MsPause { get; private set; }

    public void Enqueue( int msPause = 0 )
    {
        if( msPause == 0 )
            msPause = Importance switch
            {
                MessageLevel.Important => 1000,
                MessageLevel.Urgent => 2000,
                _ => msPause
            };

        MsPause = msPause;

        _messageQueue.Add( this );
    }
}
