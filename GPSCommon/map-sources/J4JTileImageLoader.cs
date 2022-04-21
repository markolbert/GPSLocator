using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.GPSCommon;

public class J4JTileImageLoader : ITileImageLoader
{
    private class HttpResponse
    {
        public byte[]? Buffer { get; }
        public TimeSpan? MaxAge { get; }

        public HttpResponse( byte[]? buffer, TimeSpan? maxAge )
        {
            Buffer = buffer;
            MaxAge = maxAge;
        }
    }

    public static string DefaultCacheFolder =>
        Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ),
                      "MapControl",
                      "TileCache" );

    public static MapControl.Caching.IImageCache? Cache { get; set; }
    public static int MaxLoadTasks { get; set; } = 4;
    public static TimeSpan DefaultCacheExpiration { get; set; } = TimeSpan.FromDays( 1 );
    public static TimeSpan MaxCacheExpiration { get; set; } = TimeSpan.FromDays( 10 );

    private string? _cacheName;
    private ConcurrentStack<Tile>? _pendingTiles;

    public J4JTileImageLoader(
        IJ4JLogger logger
    )
    {
        MaxLoadTasks = 1;

        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    protected IJ4JLogger Logger { get; }

    /// <summary>
    /// The current TileSource, passed to the most recent LoadTiles call.
    /// </summary>
    public TileSource? TileSource { get; private set; }

    protected BoundingBox? CurrentBoundingBox { get; private set; }

    protected virtual BoundingBox? GetBoundingBox() => null;

    /// <summary>
    /// Loads all pending tiles from the tiles collection.
    /// If tileSource.UriFormat starts with "http" and cacheName is a non-empty string,
    /// tile images will be cached in the TileImageLoader's Cache - if that is not null.
    /// </summary>
    public Task LoadTiles( IEnumerable<Tile> tiles, TileSource? tileSource, string? cacheName )
    {
        CurrentBoundingBox = GetBoundingBox();

        _pendingTiles?.Clear(); // stop processing the current queue

        TileSource = tileSource;
        _cacheName = cacheName;

        if( tileSource == null )
            return Task.CompletedTask;

        _pendingTiles = new ConcurrentStack<Tile>( tiles.Where( tile => tile.Pending ).Reverse() );

        var numTasks = Math.Min( _pendingTiles.Count, MaxLoadTasks );

        if( numTasks <= 0 )
            return Task.CompletedTask;

        _cacheName = CacheTiles() ? cacheName : null;

        var tasks = Enumerable.Range( 0, numTasks )
                              .Select( _ => Task.Run( LoadPendingTiles ) );

        return Task.WhenAll( tasks );
    }

    protected virtual bool CacheTiles() =>
        Cache != null
     && TileSource?.UriFormat != null
     && TileSource.UriFormat.StartsWith( "http" );

    private async Task LoadPendingTiles()
    {
        while( _pendingTiles!.TryPop( out var tile ) )
        {
            tile.Pending = false;

            try
            {
                await LoadTile( tile ).ConfigureAwait( false );
            }
            catch( Exception ex )
            {
                Logger.Error( "Failed to load tile : {0}/{1}/{2}: {3}",
                              new object[] { tile.ZoomLevel, tile.XIndex, tile.Y, ex.Message } );
            }
        }
    }

    protected virtual Task LoadTile( Tile tile )
    {
        if( string.IsNullOrEmpty( _cacheName ) )
            return SetTileImage( tile, () => TileSource!.LoadImageAsync( tile.XIndex, tile.Y, tile.ZoomLevel ) );

        var uri = TileSource!.GetUri( tile.XIndex, tile.Y, tile.ZoomLevel );

        if( uri == null )
            return Task.CompletedTask;

        var extension = Path.GetExtension( uri.LocalPath );

        if( string.IsNullOrEmpty( extension ) || extension == ".jpeg" )
            extension = ".jpg";

        var cacheKey = string.Format( CultureInfo.InvariantCulture,
                                      "{0}/{1}/{2}/{3}{4}",
                                      _cacheName,
                                      tile.ZoomLevel,
                                      tile.XIndex,
                                      tile.Y,
                                      extension );

        return LoadCachedTile( tile, uri, cacheKey );
    }

    private async Task LoadCachedTile( Tile tile, Uri uri, string cacheKey )
    {
        var cacheItem = await Cache!.GetAsync( cacheKey ).ConfigureAwait( false );
        var buffer = cacheItem?.Item1;

        if( cacheItem == null || cacheItem.Item2 < DateTime.UtcNow )
        {
            var response = await GetHttpResponseAsync( uri ).ConfigureAwait( false );

            if( response != null ) // download succeeded
            {
                buffer = response.Buffer; // may be null or empty when no tile available, but still be cached

                await Cache.SetAsync( cacheKey, buffer, GetExpiration( response.MaxAge ) ).ConfigureAwait( false );
            }
        }
        else Logger.Verbose<string>( "Cached: {0}", cacheKey );

        if( buffer != null && buffer.Length > 0 )
            await SetTileImage( tile, () => ImageLoader.LoadImageAsync( buffer ) ).ConfigureAwait( false );
    }

    private async Task<HttpResponse?> GetHttpResponseAsync( Uri uri )
    {
        HttpResponse? response = null;

        try
        {
            var httpClient = new HttpClient();

            using var responseMessage = await httpClient.GetAsync( uri, HttpCompletionOption.ResponseHeadersRead )
                                                        .ConfigureAwait( false );

            if( responseMessage.IsSuccessStatusCode )
            {
                byte[]? buffer = null;

                if( !responseMessage.Headers.TryGetValues( "X-VE-Tile-Info", out IEnumerable<string>? tileInfo )
                || !tileInfo.Contains( "no-tile" ) )
                    buffer = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait( false );

                response = new HttpResponse( buffer, responseMessage.Headers.CacheControl?.MaxAge );
            }
            else
                Logger.Error<Uri, HttpStatusCode, string>( "Failed to retrieve image",
                                                           uri,
                                                           responseMessage.StatusCode,
                                                           responseMessage.ReasonPhrase ?? "no reason given" );
        }
        catch( Exception ex )
        {
            Logger.Error<Uri, string>( "Retrieving image failed: {0}: {1}", uri, ex.Message );
        }

        return response;
    }

    private Task SetTileImage( Tile tile, Func<Task<ImageSource>> loadImageFunc )
    {
        var tcs = new TaskCompletionSource();

        async void Callback()
        {
            try
            {
                tile.SetImage( await loadImageFunc() );
                tcs.TrySetResult();
            }
            catch( Exception ex )
            {
                tcs.TrySetException( ex );
            }
        }

        if( tile.Image.DispatcherQueue.TryEnqueue( DispatcherQueuePriority.Low, Callback ) )
            return tcs.Task;

        tile.Pending = true;
        tcs.TrySetResult();

        return tcs.Task;
    }

    private DateTime GetExpiration( TimeSpan? maxAge )
    {
        if( !maxAge.HasValue )
            maxAge = DefaultCacheExpiration;
        else
            if( maxAge.Value > MaxCacheExpiration )
                maxAge = MaxCacheExpiration;

        return DateTime.UtcNow.Add( maxAge.Value );
    }
}