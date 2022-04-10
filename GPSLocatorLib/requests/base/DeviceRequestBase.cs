using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;

namespace J4JSoftware.GPSLocator
{
    public abstract class DeviceRequestBase<TResponse, TError>
        where TResponse : class, new()
        where TError : ErrorBase, new()
    {
        public event EventHandler<RequestEventArgs<TResponse>>? Status;

        private readonly string _svcGroup;
        private readonly string _service;
        private readonly Direction _direction;
        private readonly string _version;
        private readonly bool _requiresAuth;
        private readonly Dictionary<string, string> _queryStrings = new(StringComparer.OrdinalIgnoreCase);

        protected DeviceRequestBase(
            DeviceConfig config,
            IJ4JLogger logger,
            IBullshitLogger bsLogger
        )
        {
            if( GetType()
               .GetCustomAttributes( false )
               .FirstOrDefault( x => x is LocatorAttribute ) is not LocatorAttribute svcAttr )
                throw new NullReferenceException(
                    $"{nameof(DeviceRequestBase<TResponse, TError> )} not decorated with {nameof( LocatorAttribute )}" );

            Configuration = config;
            _svcGroup = svcAttr!.ServiceGroup;
            _service = svcAttr.Service;
            _direction = svcAttr.Direction;
            _version = svcAttr.Version;
            _requiresAuth = svcAttr.RequiresAuthentication;

            Logger = logger;
            Logger.SetLoggedType( GetType() );

            BSLogger = bsLogger;
            BSLogger.Log($"Created device request '{GetType().Name}'");
        }

        protected IJ4JLogger Logger { get; }
        protected IBullshitLogger BSLogger { get; }
        protected DeviceConfig Configuration { get; }

        public async Task<DeviceResponse<TResponse>> ExecuteAsync()
        {
            Status?.Invoke( this, new RequestEventArgs<TResponse>( RequestEvent.Started, null ) );
            BSLogger.Log("Beginning request execution");
            
            var website = Configuration.Website.Replace( "http://", "", StringComparison.OrdinalIgnoreCase )
                                       .Replace( "https://", "", StringComparison.OrdinalIgnoreCase );

            var requestUri = QueryHelpers.AddQueryString(
                $"https://{website}/{_direction}/V{_version}/{_svcGroup}/{_service}",
                _queryStrings );

            BSLogger.Log($"Uri is '{requestUri}'");

            var credentials = _requiresAuth
                ? new NetworkCredential( Configuration.UserName, Configuration.Password )
                : null;

            HttpClientHandler? clientHandler;

            try
            {
                clientHandler = new HttpClientHandler()
                {
                    Credentials = credentials?.GetCredential( new Uri( requestUri ), "Basic" )
                };

                BSLogger.Log( $"Created {typeof( HttpClientHandler )}" );
            }
            catch ( Exception ex )
            {
                BSLogger.Log( $"Failed to create {typeof( HttpClientHandler )}, message was '{ex.Message}'" );

                return CreateErrorAndAbort( requestUri, ex );
            }

            var httpClient = new HttpClient( clientHandler );

            Logger.Information<string>( "Querying {0}", requestUri );
            BSLogger.Log( $"Querying {requestUri}" );

            HttpResponseMessage? response;

            try
            {
                response = await ExecuteInternalAsync( httpClient, requestUri );
            }
            catch ( Exception ex )
            {
                BSLogger.Log( $"{nameof( ExecuteInternalAsync )}() failed, message was {ex.Message}" );

                return CreateErrorAndAbort( requestUri, ex );
            }

            if (!response.IsSuccessStatusCode)
                return await CreateErrorAndAbortAsync(requestUri, response);

            Logger.Information("Reading response");
            BSLogger.Log( "Reading response" );
            var respText = await response.Content.ReadAsStringAsync();

            var retVal = new DeviceResponse<TResponse>( requestUri );

            try
            {
                Logger.Information("Parsing response");
                BSLogger.Log("Parsing response");

                retVal.Result = JsonSerializer.Deserialize<TResponse>( respText );

                Logger.Information("Query succeeded");
                BSLogger.Log( "Query succeeded" );
            }
            catch ( Exception ex )
            {
                Logger.Error<Type, string>("Parsing response to {0} failed, message was '{1}'",
                                           typeof(TResponse),
                                           ex.Message);
                BSLogger.Log($"Parsing response to {typeof(TResponse)} failed, message was '{ex.Message}'");

                retVal.Error =
                    new TError() { Description = $"Response parsing failed, message was '{ex.Message}'" };
            }

            Status?.Invoke( this, new RequestEventArgs<TResponse>( RequestEvent.Succeeded, retVal ) );

            return retVal;
        }

#pragma warning disable CS1998
        protected virtual async Task<HttpResponseMessage> ExecuteInternalAsync(HttpClient httpClient, string requestUri)
#pragma warning restore CS1998
        {
            return new HttpResponseMessage( HttpStatusCode.BadRequest );
        }

        protected void SetQueryProperty<TProp>(
            ref TProp property,
            TProp value,
            Func<TProp, string>? toTextFunc = null,
            [CallerMemberName] string memberName = ""
        )
        {
            property = value;

            toTextFunc ??= x =>
            {
                if (x is string text)
                    return text;

                return x?.ToString() ?? string.Empty;
            };

            if (_queryStrings.ContainsKey(memberName))
                _queryStrings[memberName] = toTextFunc(value);
            else _queryStrings.Add(memberName, toTextFunc(value));
        }

        protected async Task<DeviceResponse<TResponse>> CreateErrorAndAbortAsync( string requestUri, HttpResponseMessage response )
        {
            ErrorBase? devError;
            DeviceResponse<TResponse>? retVal;

            try
            {
                var respText = await response.Content.ReadAsStringAsync();
                devError = JsonSerializer.Deserialize<TError>( respText );

                retVal = CreateErrorAndAbort( requestUri, null );
                retVal.Error = devError;
            }
            catch( Exception jsonEx )
            {
                retVal = CreateErrorAndAbort( requestUri, jsonEx );
            }

            return retVal;
        }

        protected DeviceResponse<TResponse> CreateErrorAndAbort( string uri, Exception? ex )
        {
            var retEx = new DeviceResponse<TResponse>(uri)
            {
                Error = new TError()
                {
                    Description = ex?.Message ?? string.Empty,
                    HttpResponseCode = ex is HttpRequestException reqEx
                        ? reqEx.StatusCode ?? HttpStatusCode.NotFound
                        : HttpStatusCode.NotFound
                }
            };

            Status?.Invoke(this, new RequestEventArgs<TResponse>(RequestEvent.Aborted, retEx));

            return retEx;
        }
    }
}
