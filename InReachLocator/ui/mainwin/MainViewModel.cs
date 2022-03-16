using J4JSoftware.Logging;

namespace J4JSoftware.InReach
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel( 
            IJ4JLogger logger 
            )
            : base( logger )
        {
            IsActive = true;
        }
    }
}
