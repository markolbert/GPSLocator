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
        public event EventHandler? Started;
        public event EventHandler? Ended;

        private readonly string _svcGroup;
        private readonly string _service;
        private readonly Direction _direction;
        private readonly string _version;
        private readonly bool _requiresAuth;
        private readonly Dictionary<string, string> _queryStrings = new(StringComparer.OrdinalIgnoreCase);

        protected DeviceRequestBase(
            DeviceConfig config,
            IJ4JLogger logger
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
        }

        protected IJ4JLogger Logger { get; }
        protected DeviceConfig Configuration { get; }

        public async Task<DeviceResponse<TResponse>> ExecuteAsync()
        {
            Started?.Invoke(this, EventArgs.Empty);
            
            var website = Configuration.Website.Replace( "http://", "", StringComparison.OrdinalIgnoreCase )
                                       .Replace( "https://", "", StringComparison.OrdinalIgnoreCase );

            var requestUri = QueryHelpers.AddQueryString(
                $"https://{website}/{_direction}/V{_version}/{_svcGroup}/{_service}",
                _queryStrings );

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
            }
            catch( Exception ex )
            {
                return HandleError( ex, requestUri );
            }

            var httpClient = new HttpClient( clientHandler );

            Logger.Information<string>( "Querying {0}", requestUri );

            (HttpResponseMessage? ResponseMessage, DeviceResponse<TResponse>? DeviceResponse) internalResult;

            try
            {
                internalResult = await ExecuteInternalAsync( httpClient, requestUri );
            }
            catch ( Exception ex )
            {
                return HandleError(ex, requestUri);
            }

            if (!internalResult!.ResponseMessage!.IsSuccessStatusCode)
                return await HandleInvalidResponseAsync(requestUri, internalResult.ResponseMessage);

            Logger.Information("Reading response");
            var respText = await internalResult!.ResponseMessage!.Content.ReadAsStringAsync();

            var retVal = new DeviceResponse<TResponse>( requestUri );

            try
            {
                Logger.Information("Parsing response");

                retVal.Result = JsonSerializer.Deserialize<TResponse>( respText );

                Logger.Information("Query succeeded");
            }
            catch ( Exception ex )
            {
                HandleContentParsingError(retVal, ex);
            }

            Ended?.Invoke(this, EventArgs.Empty);

            return retVal;
        }

#pragma warning disable CS1998
        protected virtual async Task<(HttpResponseMessage?, DeviceResponse<TResponse>?)> ExecuteInternalAsync(HttpClient httpClient, string requestUri)
#pragma warning restore CS1998
        {
            return ( null, null );
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

        protected DeviceResponse<TResponse> HandleError( Exception? ex, string requestUri )
        {
            Logger.Error<string?>(
                $"{_direction}/V{_version}/{_svcGroup}/{_version} request failed, message was {0}",
                ex?.Message);

            var retVal = new DeviceResponse<TResponse>( requestUri ) { 
                Error = new TError()
                {
                    Description = ex?.Message ?? string.Empty,
                    HttpResponseCode = ex is HttpRequestException reqEx
                        ? reqEx.StatusCode ?? HttpStatusCode.NotFound
                        : HttpStatusCode.NotFound
                }
            };

            Ended?.Invoke(this, EventArgs.Empty);

            return retVal;
        }

        protected async Task<DeviceResponse<TResponse>> HandleInvalidResponseAsync( string requestUri, HttpResponseMessage response )
        {
            ErrorBase? devError;
            DeviceResponse<TResponse>? retVal;

            try
            {
                var respText = await response.Content.ReadAsStringAsync();
                devError = JsonSerializer.Deserialize<TError>( respText );

                retVal = HandleError( null, requestUri );
                retVal.Error = devError;
            }
            catch( Exception jsonEx )
            {
                retVal = HandleError( jsonEx, requestUri );
            }

            Ended?.Invoke( this, EventArgs.Empty );

            return retVal;
        }

        protected void HandleContentParsingError(DeviceResponse<TResponse> response, Exception ex )
        {
            response.Error =
                new TError() { Description = $"Response parsing failed, message was '{ex.Message}'" };

            Logger.Error<Type, string>("Parsing response to {0} failed, message was '{1}'",
                                       typeof(TResponse),
                                       ex.Message);
        }
    }
}
