using J4JSoftware.GPSCommon;

namespace J4JSoftware.GPSLocator;

public record MapViewModelMessage<TAppConfig>(LocationMapViewModel<TAppConfig> ViewModel )
    where TAppConfig : BaseAppConfig;