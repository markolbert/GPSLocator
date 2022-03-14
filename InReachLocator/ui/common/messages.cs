using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.InReach
{
    public record ConfigurationChangedMessage(bool Changed);

    public record UpdateColumnWidthMessage();

    public record SizeMessage(double Height, double Width)
    {
        public SizeMessage(Size size)
            : this(size.Height, size.Width)
        {
        }
    }
}
