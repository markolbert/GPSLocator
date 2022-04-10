using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;

namespace J4JSoftware.GPSLocator
{
    public class DeviceGetRequest<TResponse, TError> : DeviceRequestBase<TResponse, TError>
        where TResponse : class, new()
        where TError : ErrorBase, new()
    {
        protected DeviceGetRequest(
            DeviceConfig config,
            IJ4JLogger logger,
            IBullshitLogger bsLogger
        )
            : base( config, logger, bsLogger )
        {
        }

        protected override async Task<(HttpResponseMessage?, DeviceResponse<TResponse>?)> ExecuteInternalAsync(
            HttpClient httpClient,
            string requestUri
        )
        {
            Logger.Information<string>( "Querying {0}", requestUri );
            BSLogger.Log($"Querying '{requestUri}'"  );

            HttpResponseMessage? response;

            try
            {
                response = await httpClient.GetAsync( requestUri );
                BSLogger.Log($"Got response");
            }
            catch ( Exception ex )
            {
                return ( null, HandleError( ex, requestUri ) );
            }

            return (response, null);
        }
    }
}
