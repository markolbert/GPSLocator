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
    public class InReachRequest<TResponse>
        where TResponse : class
    {
        private readonly Dictionary<string, string> _queryStrings = new( StringComparer.OrdinalIgnoreCase );

        protected InReachRequest(
            IInReachConfig config,
            IJ4JLogger logger
        )
        {
            if( GetType()
               .GetCustomAttributes( false )
               .FirstOrDefault( x => x is InReachAttribute ) is not InReachAttribute svcAttr )
                throw new NullReferenceException( $"{nameof(InReachRequest<TResponse>)} not decorated with {nameof(InReachAttribute)}" );

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
        protected IInReachConfig Configuration { get; }
        protected bool RequiresAuthentication { get; }

        public string? LastUri { get; private set; }
        public InReachError? LastError { get; protected set; }

        //protected void SetQueryProperty( ref string property, string value, [ CallerMemberName ] string memberName = "" )
        //{
        //    property = value;

        //    if (_queryStrings.ContainsKey(memberName))
        //        _queryStrings[memberName] = value;
        //    else _queryStrings.Add(memberName, value);
        //}

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

        public async Task<TResponse?> ExecuteAsync()
        {
            LastError = null;

            LastUri = QueryHelpers.AddQueryString(
                $"https://{Configuration.Website}/{Direction}/V{Version}/{ServiceGroup}/{Service}",
                _queryStrings );

            var credentials = new NetworkCredential(Configuration.UserName, Configuration.Password.ClearText);

            var clientHandler = new HttpClientHandler()
            {
                Credentials = credentials.GetCredential( new Uri( LastUri ), "Basic" )
            };

            var httpClient = new HttpClient( clientHandler );

            Logger.Information<string>("Querying {0}", LastUri);
            var response = await httpClient.GetAsync( LastUri );

            Logger.Information("Reading response");
            var respText = await response.Content.ReadAsStringAsync();

            if ( !response.IsSuccessStatusCode )
            {
                Logger.Error<string?>(
                    $"{Direction}/V{Version}/{ServiceGroup}/{Version} request failed, message was {0}",
                    response.ReasonPhrase );

                LastError = JsonSerializer.Deserialize<InReachError>(respText);
                
                if( LastError != null )
                    LastError.HttpResponseCode = response.StatusCode;

                return null;
            }

            TResponse? retVal = null;

            try
            {
                Logger.Information("Parsing response");
                retVal = JsonSerializer.Deserialize<TResponse>( respText );

                Logger.Information("Query succeeded");
            }
            catch ( Exception ex )
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
