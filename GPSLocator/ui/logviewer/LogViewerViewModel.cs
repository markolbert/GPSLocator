using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog.Events;

namespace J4JSoftware.GPSLocator
{
    public class LogViewerViewModel : BaseViewModel
    {
        private readonly AppViewModel _appViewModel;

        private LogEventLevel _minLevel = LogEventLevel.Verbose;

        public LogViewerViewModel(
            IJ4JLogger logger
        )
            : base( logger )
        {
            _appViewModel = ( App.Current.Resources[ "AppViewModel" ] as AppViewModel )!;

            FilteredLogEvents = new ObservableCollection<IndexedLogEvent>( _appViewModel.LogEvents );

            LogLevels = Enum.GetValues<LogEventLevel>().ToList();
            ClearLogCommand = new RelayCommand( ClearLogHandler );
        }

        public void OnPageActivated()
        {
            MinimumLogEventLevel = _appViewModel.Configuration.MinimumLogLevel;
        }

        public List<LogEventLevel> LogLevels { get; }

        public RelayCommand ClearLogCommand { get; }

        private void ClearLogHandler() => _appViewModel.LogEvents.Clear();

        public ObservableCollection<IndexedLogEvent> FilteredLogEvents { get; }

        public LogEventLevel MinimumLogEventLevel
        {
            get => _minLevel;

            set
            {
                var changed = _minLevel != value;

                SetProperty( ref _minLevel, value );

                if( changed )
                    UpdateFilteredList();
            }
        }

        private void UpdateFilteredList()
        {
            var indicesToRemove = FilteredLogEvents
                                 .Where( x => x.LogEventLevel < MinimumLogEventLevel )
                                 .Select( x => x.Index )
                                 .OrderByDescending( x => x )
                                 .ToList();

            foreach( var idx in indicesToRemove )
            {
                FilteredLogEvents.RemoveAt(idx);
            }

            var newItems = _appViewModel.LogEvents
                                     .Where( x => x.LogEventLevel >= MinimumLogEventLevel
                                              && FilteredLogEvents.All( y => y.Index != x.Index ) )
                                     .ToList();

            foreach( var newItem in newItems )
            {
                var prevItem = FilteredLogEvents.Where( ( x, i ) => x.Index < newItem.Index )
                                                .Select( ( x, i ) => new { FilteredIndex = i, Item = x } )
                                                .FirstOrDefault();

                if( prevItem == null )
                    FilteredLogEvents.Add( newItem );
                else
                    FilteredLogEvents.Insert( prevItem.FilteredIndex + 1, newItem );
            }
        }
    }
}
