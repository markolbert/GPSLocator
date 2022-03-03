using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;

namespace J4JSoftware.InReach
{
    public class InReachRequest<TResponse>
        where TResponse : class
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _queryStrings =
            new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        protected InReachRequest(
            IInReachConfig config,
            IJ4JLogger logger
        )
        {
            var svcAttr = GetType()
                            .GetCustomAttributes( false )
                            .FirstOrDefault( x => x is InReachRequestAttribute )
                as InReachRequestAttribute;

            if( svcAttr == null )
                throw new NullReferenceException( $"{nameof(InReachRequest<TResponse>)} not decorated with {nameof(InReachRequestAttribute)}" );

            Configuration = config;
            ServiceGroup = svcAttr!.ServiceGroup;
            Service = svcAttr.Service;
            Direction = svcAttr.Direction;
            Version = svcAttr.Version;

            BaseUri = $"https://{Configuration.Website}/{Direction}/V{Version}/{ServiceGroup}/{Service}";

            _httpClient = new HttpClient();

            if( svcAttr.RequiresAuthentication )
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue( "Basic", Configuration.Credentials );

            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }

        protected string ServiceGroup { get; }
        protected string Service { get; }
        protected Direction Direction { get; }
        protected string Version { get; }
        protected IInReachConfig Configuration { get; }

        public string BaseUri { get; }
        public InReachError? LastError { get; protected set; }

        protected void AddQueryString( string name, string value )
        {
            if( _queryStrings.ContainsKey( name ) )
                _queryStrings[ name ] = value;
            else _queryStrings.Add( name, value );
        }

        public virtual async Task<TResponse?> ExecuteAsync()
        {
            LastError = null;

            var uri = QueryHelpers.AddQueryString( BaseUri, _queryStrings );

            var response = await _httpClient.GetAsync( uri );
            var respText = await response.Content.ReadAsStringAsync();

            if ( !response.IsSuccessStatusCode )
            {
                Logger.Error<string?>(
                    $"{Direction}/V{Version}/{ServiceGroup}/{Version} request failed, message was {0}",
                    response.ReasonPhrase );

                LastError = JsonSerializer.Deserialize<InReachError>(respText);

                return null;
            }

            TResponse? retVal = null;

            try
            {
                retVal = JsonSerializer.Deserialize<TResponse>( respText );
            }
            catch( Exception ex )
            {
                Logger.Error<Type, string>( "Parsing response to {0} failed, message was '{1}'",
                                            typeof( TResponse ),
                                            ex.Message );
            }

            return retVal;
        }
    }

    public class InReachVersion
    {
        public string Service { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public string Build { get; set; } = string.Empty;

    }
}
