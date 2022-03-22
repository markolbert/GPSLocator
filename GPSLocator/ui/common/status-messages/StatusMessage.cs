namespace J4JSoftware.GPSLocator;

public partial class StatusMessage
{
    private readonly StatusMessages _container;

    private ProgressBarType? _progressBarType;

    private StatusMessage(
        string text,
        StatusMessages container
    )
    {
        Text = text;
        _container = container;
    }

    public StatusMessage Copy()
    {
        return new StatusMessage( Text, _container )
        {
            ProgressBarType = _progressBarType,
            MaxProgressBar = MaxProgressBar,
            Importance = Importance,
            Pause = Pause
        };
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

    public (int Minimum, int Maximum) Pause { get; private set; }

    public StatusMessages Enqueue(int maxPause = 0, int minPause = 0)
    {
        Pause = NormalizeMinMax(minPause, maxPause);

        _container.Add(this);

        return _container;
    }

    public void Display( int maxPause = 0, int minPause = 0 )
    {
        Pause = NormalizeMinMax( minPause, maxPause );

        _container.Add( this );
        _container.Display();
    }

    private (int minPause, int maxPause) NormalizeMinMax(int minPause, int maxPause)
    {
        if (minPause <= 0)
            minPause = StatusMessages.DefaultMinimumPause;

        if (maxPause <= 0)
        {
            maxPause = Importance switch
            {
                MessageLevel.Important => 1000,
                MessageLevel.Urgent => 2000,
                _ => maxPause
            };
        }

        return maxPause >= minPause
            ? (minPause, maxPause)
            : (maxPause, minPause);
    }
}
