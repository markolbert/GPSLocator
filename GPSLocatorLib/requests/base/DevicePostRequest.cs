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
            DeviceConfig config,
            IJ4JLogger logger,
            IBullshitLogger bsLogger
        )
            : base( config, logger, bsLogger )
        {
        }

        protected override async Task<HttpResponseMessage> ExecuteInternalAsync(
            HttpClient httpClient,
            string requestUri
        )
        {
            Logger.Information<string>( "Querying {0}", requestUri );
            BSLogger.Log($"Querying '{requestUri}'");

            HttpResponseMessage? response;

            try
            {
                var postObj = GetJsonPostObject();
                var postReq = new HttpRequestMessage( HttpMethod.Post, requestUri );

                var jsonText = JsonSerializer.Serialize( postObj );
                postReq.Content = new StringContent( jsonText, Encoding.UTF8, "application/json" );

                response = await httpClient.SendAsync( postReq );
                BSLogger.Log("Got response");
            }
            catch( Exception ex )
            {
                return new HttpResponseMessage( HttpStatusCode.BadRequest );
            }

            return response;
        }

        protected abstract object GetJsonPostObject();
    }
}
