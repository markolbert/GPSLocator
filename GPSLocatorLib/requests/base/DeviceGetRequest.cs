using System.Net;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator
{
    public class DeviceGetRequest<TResponse, TError> : DeviceRequestBase<TResponse, TError>
        where TResponse : class, new()
        where TError : ErrorBase, new()
    {
        protected DeviceGetRequest(
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
                response = await httpClient.GetAsync( requestUri );
                Logger.Information("Got response");
            }
            catch ( Exception )
            {
                return new HttpResponseMessage( HttpStatusCode.BadRequest );
            }

            return response;
        }
    }
}
