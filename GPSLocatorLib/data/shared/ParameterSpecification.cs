namespace J4JSoftware.GPSLocator;

public class ParameterSpecification
{
    public string AcceptedValues { get; set; } = string.Empty;

    public List<string> AcceptedValuesList =>
        string.IsNullOrEmpty( AcceptedValues )
            ? new List<string>()
            : AcceptedValues.Split( ',' ).ToList();

    public string AcceptedValuesDesc { get; set; } = string.Empty;

    public List<string> AcceptedValuesDescList =>
        string.IsNullOrEmpty(AcceptedValuesDesc)
            ? new List<string>()
            : AcceptedValuesDesc.Split(';').ToList();
    
    public bool BundleTrackEnabled { get; set; }
    public string Category {get; set; } = string.Empty;
    public bool ConfigEditable { get; set; }
    public bool ConfigRemovable { get; set; }
    public bool ConsumerEnabled { get; set; }
    public string DefaultConfig { get; set; } = string.Empty;

    public List<string> DefaultConfigList =>
        string.IsNullOrEmpty(DefaultConfig)
            ? new List<string>()
            : DefaultConfig.Split(',').ToList();

    public string DefaultSetting { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long DeviceKey { get; set; }
    public long DeviceTypes { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool EnterpriseEnabled { get; set; }
    public bool ExploreExposed { get; set; }
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UnitString { get; set; }
}
