using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;

namespace J4JSoftware.GPSLocator
{
    public class DeviceRequest<T>
        where T : class, new()
    {
        public event EventHandler? Started;
        public event EventHandler? Ended;

        private readonly Dictionary<string, string> _queryStrings = new( StringComparer.OrdinalIgnoreCase );

        protected DeviceRequest(
            DeviceConfig config,
            IJ4JLogger logger
        )
        {
            if( GetType()
               .GetCustomAttributes( false )
               .FirstOrDefault( x => x is LocatorAttribute ) is not LocatorAttribute svcAttr )
                throw new NullReferenceException( $"{nameof(DeviceRequest<T>)} not decorated with {nameof(LocatorAttribute)}" );

            Configuration = config;
            ServiceGroup = svcAttr!.ServiceGroup;
            Service = svcAttr.Service;
            Direction = svcAttr.Direction;
            Version = svcAttr.Version;
            RequiresAuthentication = svcAttr.RequiresAuthentication;

            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }

        protected string ServiceGroup { get; }
        protected string Service { get; }
        protected Direction Direction { get; }
        protected string Version { get; }
        protected DeviceConfig Configuration { get; }
        protected bool RequiresAuthentication { get; }

        protected void SetQueryProperty<TProp>(
            ref TProp property,
            TProp value,
            Func<TProp, string>? toTextFunc = null,
            [ CallerMemberName ] string memberName = ""
        )
        {
            property = value;

            toTextFunc ??= x =>
            {
                if( x is string text )
                    return text;

                return x?.ToString() ?? string.Empty;
            };

            if( _queryStrings.ContainsKey( memberName ) )
                _queryStrings[ memberName ] = toTextFunc( value );
            else _queryStrings.Add( memberName, toTextFunc( value ) );
        }

        public async Task<DeviceResponse<T>> ExecuteAsync()
        {
            Started?.Invoke(this, EventArgs.Empty);
            
            var website = Configuration.Website.Replace( "http://", "", StringComparison.OrdinalIgnoreCase )
                                       .Replace( "https://", "", StringComparison.OrdinalIgnoreCase );

            var retVal = new DeviceResponse<T>( QueryHelpers.AddQueryString(
                                                     $"https://{website}/{Direction}/V{Version}/{ServiceGroup}/{Service}",
                                                     _queryStrings ) );

            var credentials = RequiresAuthentication
                ? new NetworkCredential( Configuration.UserName, Configuration.Password )
                : null;

            HttpClientHandler? clientHandler;

            try
            {
                clientHandler = new HttpClientHandler()
                {
                    Credentials = credentials?.GetCredential( new Uri( retVal.RequestUri ), "Basic" )
                };
            }
            catch( Exception ex )
            {
                HandleError( retVal, ex );
                return retVal;
            }

            var httpClient = new HttpClient( clientHandler );

            Logger.Information<string>( "Querying {0}", retVal.RequestUri );

            HttpResponseMessage? response;

            try
            {
                response = await httpClient.GetAsync( retVal.RequestUri );
            }
            catch ( Exception ex )
            {
                HandleError(retVal, ex);
                return retVal;
            }

            Logger.Information("Reading response");
            var respText = await response.Content.ReadAsStringAsync();

            if( !response.IsSuccessStatusCode )
            {
                HandleInvalidResponse(retVal, response, respText);
                return retVal;
            }

            try
            {
                Logger.Information("Parsing response");

                retVal.Result = JsonSerializer.Deserialize<T>( respText );

                Logger.Information("Query succeeded");
            }
            catch ( Exception ex )
            {
                HandleContentParsingError(retVal, ex);
            }

            Ended?.Invoke(this, EventArgs.Empty);

            return retVal;
        }

        private void HandleError( DeviceResponse<T> response, Exception ex )
        {
            Logger.Error<string?>(
                $"{Direction}/V{Version}/{ServiceGroup}/{Version} request failed, message was {0}",
                ex.Message);

            response.Error = new DeviceError()
            {
                Description = ex.Message,
                HttpResponseCode = ex is HttpRequestException reqEx
                    ? reqEx.StatusCode ?? HttpStatusCode.NotFound
                    : HttpStatusCode.NotFound
            };

            Ended?.Invoke(this, EventArgs.Empty);
        }

        private void HandleInvalidResponse(DeviceResponse<T> response, HttpResponseMessage httpResponse, string respText )
        {
            Logger.Error<string?>(
                $"{Direction}/V{Version}/{ServiceGroup}/{Version} request failed, message was {0}",
                httpResponse.ReasonPhrase);

            try
            {
                response.Error = JsonSerializer.Deserialize<DeviceError>(respText);
            }
            catch (Exception ex)
            {
                response.Error = new DeviceError()
                {
                    Description = $"Failed to parse response, message was '{ex.Message}'"
                };
            }

            if (response.Error != null)
                response.Error.HttpResponseCode = httpResponse.StatusCode;

            Ended?.Invoke(this, EventArgs.Empty);
        }

        private void HandleContentParsingError(DeviceResponse<T> response, Exception ex )
        {
            response.Error =
                new DeviceError { Description = $"Response parsing failed, message was '{ex.Message}'" };

            Logger.Error<Type, string>("Parsing response to {0} failed, message was '{1}'",
                                       typeof(T),
                                       ex.Message);
        }
    }
}
