namespace J4JSoftware.GPSLocator;

public partial class StatusMessages
{
    public class StatusMessage
    {
        private readonly StatusMessages _container;

        private ProgressBarType? _progressBarType;

        internal StatusMessage(
            string text,
            StatusMessages container
        )
        {
            Text = text;
            _container = container;
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

        public StatusMessage Determinate(int max)
        {
            _progressBarType = GPSLocator.ProgressBarType.Determinate;
            MaxProgressBar = max;

            return this;
        }

        public MessageLevel Importance { get; private set; } = MessageLevel.Normal;

        public StatusMessage Important() => Level(MessageLevel.Important);
        public StatusMessage Urgent() => Level(MessageLevel.Urgent);

        public StatusMessage Level(MessageLevel level)
        {
            Importance = level;
            return this;
        }

        public int Pause { get; private set; }

        public void Display( int msPause = 0)
        {
            if (msPause == 0)
                msPause = Importance switch
                {
                    MessageLevel.Important => 1000,
                    MessageLevel.Urgent => 2000,
                    _ => msPause
                };

            Pause = msPause;

            _container._queued.Add(this);

            _container.Display();
        }

        public StatusMessages Enqueue(int msPause = 0)
        {
            if (msPause == 0)
                msPause = Importance switch
                {
                    MessageLevel.Important => 1000,
                    MessageLevel.Urgent => 2000,
                    _ => msPause
                };

            Pause = msPause;

            _container._queued.Add(this);

            return _container;
        }
    }
}
