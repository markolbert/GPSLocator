namespace J4JSoftware.GPSLocator
{
    public class AnnotatedLocationType
    {
        public AnnotatedLocationType(
            LocationType locationType,
            string? label = null
        )
        {
            LocationType = locationType;
            Label = label ?? locationType.ToString();
        }

        public LocationType LocationType { get; }
        public string Label { get; }
    }
}
