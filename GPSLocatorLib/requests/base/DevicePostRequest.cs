using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace J4JSoftware.GPSLocator
{
    public abstract class DevicePostRequest<TResponse, TError> : DeviceRequestBase<TResponse, TError>
        where TResponse : class, new()
        where TError : ErrorBase, new()
    {
        protected DevicePostRequest(
            IDeviceConfig config,
            IJ4JLogger logger
        )
            : base( config, logger )
        {
        }

        protected override async Task<HttpResponseMessage> ExecuteInternalAsync(
            HttpClient httpClient,
            string requestUri
        )
        {
            Logger.Information<string>( "Querying {0}", requestUri );

            HttpResponseMessage? response;

            try
            {
                var postObj = GetJsonPostObject();
                var postReq = new HttpRequestMessage( HttpMethod.Post, requestUri );

                var jsonText = JsonSerializer.Serialize( postObj );
                postReq.Content = new StringContent( jsonText, Encoding.UTF8, "application/json" );

                response = await httpClient.SendAsync( postReq );
                Logger.Information("Got response");
            }
            catch( Exception ex )
            {
                Logger.Error<Type, string>( "Post request threw exception '{0}', message was '{1}'",
                                            ex.GetType(),
                                            ex.Message );
                return new HttpResponseMessage( HttpStatusCode.BadRequest );
            }

            return response;
        }

        protected abstract object GetJsonPostObject();
    }
}
