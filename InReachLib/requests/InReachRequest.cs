using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;

namespace J4JSoftware.InReach
{
    public class InReachRequest<T>
        where T : class, new()
    {
        public event EventHandler? Started;
        public event EventHandler? Ended;

        private readonly Dictionary<string, string> _queryStrings = new( StringComparer.OrdinalIgnoreCase );

        protected InReachRequest(
            InReachConfig config,
            IJ4JLogger logger
        )
        {
            if( GetType()
               .GetCustomAttributes( false )
               .FirstOrDefault( x => x is InReachAttribute ) is not InReachAttribute svcAttr )
                throw new NullReferenceException( $"{nameof(InReachRequest<T>)} not decorated with {nameof(InReachAttribute)}" );

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
        protected InReachConfig Configuration { get; }
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

        public async Task<InReachResponse<T>> ExecuteAsync()
        {
            Started?.Invoke(this, EventArgs.Empty);
            
            var website = Configuration.Website.Replace( "http://", "", StringComparison.OrdinalIgnoreCase )
                                       .Replace( "https://", "", StringComparison.OrdinalIgnoreCase );

            var retVal = new InReachResponse<T>( QueryHelpers.AddQueryString(
                                                     $"https://{website}/{Direction}/V{Version}/{ServiceGroup}/{Service}",
                                                     _queryStrings ) );

            var credentials = RequiresAuthentication
                ? new NetworkCredential( Configuration.UserName, Configuration.Password )
                : null;

            var clientHandler = new HttpClientHandler()
            {
                Credentials = credentials?.GetCredential( new Uri( retVal.RequestUri ), "Basic" )
            };

            var httpClient = new HttpClient( clientHandler );

            Logger.Information<string>( "Querying {0}", retVal.RequestUri );

            HttpResponseMessage? response;

            try
            {
                response = await httpClient.GetAsync( retVal.RequestUri );
            }
            catch( HttpRequestException httpEx )
            {
                Logger.Error<string?>(
                    $"{Direction}/V{Version}/{ServiceGroup}/{Version} request failed, message was {0}",
                    httpEx.Message);

                retVal.Error = new InReachError()
                {
                    Description = httpEx.Message, 
                    HttpResponseCode = httpEx.StatusCode ?? HttpStatusCode.NotFound
                };

                Ended?.Invoke(this, EventArgs.Empty  );

                return retVal;
            }
            catch ( Exception ex )
            {
                retVal.Error = new InReachError()
                {
                    Description = ex.Message,
                    HttpResponseCode = HttpStatusCode.BadRequest
                };

                Logger.Error<string?>(
                    $"{Direction}/V{Version}/{ServiceGroup}/{Version} request failed, message was {0}",
                    ex.Message);

                Ended?.Invoke(this, EventArgs.Empty);

                return retVal;
            }

            Logger.Information("Reading response");
            var respText = await response.Content.ReadAsStringAsync();

            if ( !response.IsSuccessStatusCode )
            {
                Logger.Error<string?>(
                    $"{Direction}/V{Version}/{ServiceGroup}/{Version} request failed, message was {0}",
                    response.ReasonPhrase );

                try
                {
                    retVal.Error = JsonSerializer.Deserialize<InReachError>( respText );
                }
                catch( Exception ex )
                {
                    retVal.Error = new InReachError()
                    {
                        Description = $"Failed to parse response, message was '{ex.Message}'"
                    };
                }

                if( retVal.Error != null )
                    retVal.Error.HttpResponseCode = response.StatusCode;

                Ended?.Invoke(this, EventArgs.Empty);

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
                retVal.Error =
                    new InReachError { Description = $"Response parsing failed, message was '{ex.Message}'" };

                Logger.Error<Type, string>( "Parsing response to {0} failed, message was '{1}'",
                                            typeof( T ),
                                            ex.Message );
            }

            Ended?.Invoke(this, EventArgs.Empty);

            return retVal;
        }
    }
}
