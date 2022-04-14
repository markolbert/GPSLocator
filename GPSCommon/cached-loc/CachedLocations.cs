using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.GPSLocator;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Location = J4JSoftware.GPSLocator.Location;

namespace J4JSoftware.GPSCommon
{
    public class CachedLocations
    {
        public event EventHandler<CachedLocationEventArgs>? CacheChanged;
        public event EventHandler? TimeSpanChanged;

        private readonly DispatcherQueue _dQueue;
        private readonly BaseAppConfig _appConfig;
        private readonly IJ4JLogger _logger;

        private double _daysBack = 7;
        private bool _updating = false;

        public CachedLocations(
            BaseAppConfig appConfig,
            IJ4JLogger logger
        )
        {
            _appConfig = appConfig;
            _dQueue = DispatcherQueue.GetForCurrentThread();

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, object e)
        {
            EndDate = DateTimeOffset.Now;
            TimeSpanChanged?.Invoke( this, EventArgs.Empty );
        }

        public DateTimeOffset EndDate { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset StartDate => EndDate.DateTime.AddDays( -DaysBack );
        public bool Executed { get; private set; }
        public DateTime? LastExecution { get; private set; }

        public double DaysBack
        {
            get => _daysBack;

            set
            {
                if( value <= 0 )
                {
                    _logger.Warning( "Ignoring attempt to set DaysBack to a number <= 0 ({0})", value );
                    return;
                }

                var changed = Math.Abs( value - _daysBack ) > 0.001;

                _daysBack = value;

                if( !changed )
                    return;

                TimeSpanChanged?.Invoke(this, EventArgs.Empty);
                BeginUpdate();
            }
        }

        //public List<ILocation> Locations { get; } = new();
        public List<MapPoint> MapPoints { get; } = new();

        public bool BeginUpdate()
        {
            if (!_appConfig.IsValid)
            {
                _logger.Error("Configuration is invalid, cannot retrieve locations");
                return false;
            }

            if( _updating )
            {
                _logger.Warning("Location retrieval underway, cannot begin a new one");
                return false;
            }

            var request = new HistoryRequest<Location>(_appConfig, _logger)
            {
                Start = StartDate.UtcDateTime,
                End = EndDate.UtcDateTime
            };

            request.Status += OnRequestStatusChanged;

            Task.Run(async () =>
            {
                await request.ExecuteAsync();
                request.Status -= OnRequestStatusChanged;
            });

            return true;
        }

        private void OnRequestStatusChanged( object? sender, RequestEventArgs<History<Location>> args )
        {
            _dQueue.TryEnqueue( () =>
            {
                switch( args.RequestEvent )
                {
                    case RequestEvent.Started:
                        OnStarted();
                        break;

                    case RequestEvent.Succeeded:
                        OnSucceeded( args );
                        break;

                    case RequestEvent.Aborted:
                        OnAborted( args );
                        break;

                    default:
                        throw new InvalidEnumArgumentException(
                            $"Unsupported {typeof( RequestEvent )} '{args.RequestEvent}'" );
                }
            } );
        }

        private void OnStarted()
        {
            _logger.Information("Started retrieving locations");
            CacheChanged?.Invoke( this, new CachedLocationEventArgs( RequestEvent.Started ) );
        }

        private void OnSucceeded(RequestEventArgs<History<Location>> args)
        {
            MapPoints.Clear();
            MapPoints.AddRange( args.Response!.Result!.HistoryItems.Select( x => new MapPoint( x ) ) );

            _logger.Information( "Retrieved {0} locations", MapPoints.Count );

            CacheChanged?.Invoke( this,
                                  new CachedLocationEventArgs( RequestEvent.Succeeded )
                                  {
                                      Message = $"Retrieved {MapPoints.Count:n0} locations"
                                  } );

            _updating = false;
            Executed = true;
            LastExecution = DateTime.Now;
        }

        private void OnAborted(RequestEventArgs<History<Location>> args)
        {
            _logger.Warning( "Location retrieval aborted" );

            CacheChanged?.Invoke( this, new CachedLocationEventArgs( RequestEvent.Aborted ) );

            if (args.Response?.Error != null)
                _logger.Error<string>("Invalid configuration, message was '{0}'", args.Response.Error.Description);
            else _logger.Error("Invalid configuration");

            _updating = false;
        }
    }
}
