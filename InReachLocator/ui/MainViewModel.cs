using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.InReach
{
    public class MainViewModel : BasePassiveViewModel
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
