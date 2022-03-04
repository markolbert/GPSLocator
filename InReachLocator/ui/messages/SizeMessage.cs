using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace J4JSoftware.InReach
{
    public record SizeMessage( double Height, double Width )
    {
        public SizeMessage( Size size )
            : this(size.Height, size.Width)
        {
        }
    }
}
