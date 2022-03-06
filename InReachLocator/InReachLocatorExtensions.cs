using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.InReach
{
    public static class InReachLocatorExtensions
    {
        public static async Task<bool> ValidateConfiguration( this IInReachConfig config, IJ4JLogger logger )
        {
            var testReq = new LastKnownLocationRequest<Location>(config, logger);
            var result = await testReq.ExecuteAsync();

            if( result != null && result.Locations.Count != 0 )
                return true;

            if (testReq.LastError != null)
                logger.Error<string>("Invalid configuration, message was '{0}'", testReq.LastError.ToString());
            else logger.Error("Invalid configuration");

            return false;
        }
    }
}
