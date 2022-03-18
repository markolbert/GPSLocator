using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;

namespace J4JSoftware.GPSLocator
{
    public class DeviceGetRequest<TResponse> : DeviceRequestBase<TResponse>
        where TResponse : class, new()
    {
        protected DeviceGetRequest(
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
                response = await httpClient.GetAsync( requestUri );
            }
            catch( Exception ex )
            {
                return ( null, HandleError( ex, requestUri ) );
            }

            return ( response.IsSuccessStatusCode ? response : null, null );
        }
    }
}
