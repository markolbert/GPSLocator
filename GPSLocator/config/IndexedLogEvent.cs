using System.Collections.ObjectModel;
using J4JSoftware.Logging;
using Serilog.Events;

namespace J4JSoftware.GPSLocator;

public class IndexedLogEvent
{
    public class Collection : Collection<IndexedLogEvent>
    {
        public void AddLogEvent( NetEventArgs args )
        {
            Add( new IndexedLogEvent( args, this ) );
        }

        protected override void ClearItems()
        {
            foreach( var item in this )
            {
                item.Index = -1;
                item.BelongsTo = null;
            }

            base.ClearItems();
        }

        protected override void RemoveItem( int index )
        {
            this[index].BelongsTo = null;
            this[ index ].Index = -1;

            base.RemoveItem( index );
        }

        protected override void SetItem( int index, IndexedLogEvent item )
        {
            if( index < this.Count )
            {
                this[index].Index = -1;
                this[ index ].BelongsTo = null;
            }

            item.Index = index;
            item.BelongsTo = this;

            base.SetItem( index, item );
        }

        protected override void InsertItem( int index, IndexedLogEvent item )
        {
            for( var idx = index; idx < this.Count; idx++ )
            {
                this[idx].Index = idx + 1;
            }

            item.Index = index;
            item.BelongsTo = this;

            base.InsertItem( index, item );
        }
    }

    private IndexedLogEvent(
        NetEventArgs logEvent,
        Collection collection
        )
    {
        BelongsTo = collection;
        LogEventLevel = logEvent.LogEvent.Level;
        Message = logEvent.LogMessage;
    }

    public Collection? BelongsTo { get; private set; }
    public int Index { get; private set; }
    public LogEventLevel LogEventLevel { get; private set; } 
    public string Message { get; private set; }
}
