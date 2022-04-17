using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.GPSCommon;

public abstract class MapService : IMapService
{
    protected MapService(
        IBaseAppConfig appConfig,
        MapServiceType serviceType,
        IJ4JLogger logger
    )
    {
        ServiceType = serviceType;

        ApiKey = appConfig.MapCredentials.FirstOrDefault(x => x.ServiceType == serviceType)?.ApiKey;

        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    protected IJ4JLogger Logger { get; }
    protected string? ApiKey { get; }

    public MapServiceType ServiceType { get; }

    public abstract IEnumerable<MapServiceInfo> GetMapServices();
}
