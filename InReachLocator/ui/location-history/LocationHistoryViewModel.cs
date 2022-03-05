using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.InReach
{
    public class LocationHistoryViewModel : ObservableRecipient
    {
        private readonly IJ4JLogger _logger;

        public LocationHistoryViewModel(
            IJ4JLogger logger
        )
        {
            _logger = logger;
            _logger.SetLoggedType( GetType() );

            IsActive = true;
        }
    }
}
