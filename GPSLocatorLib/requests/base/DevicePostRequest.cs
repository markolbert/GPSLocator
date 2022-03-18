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
        where TError : GarminErrorBase, new()
    {
        protected DevicePostRequest(
            DeviceConfig config,
            IJ4JLogger logger
        )
            : base( config, logger )
        {
        }

        protected override async Task<(HttpResponseMessage?, DeviceResponse<TResponse>?)> ExecuteInternalAsync(
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
            }
            catch( Exception ex )
            {
                return ( null, HandleError( ex, requestUri ) );
            }

            return ( response, null );
        }

        protected abstract object GetJsonPostObject();
    }
}
