using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace J4JSoftware.InReach
{
    public class LastKnownViewModel : LocationMapViewModel
    {
        public LastKnownViewModel(
            IJ4JLogger logger
        )
        : base(logger)
        {
            IsActive = true;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<LastKnownViewModel, Location, string>(this, "primary", NewLocationHandler);
        }

        private void NewLocationHandler( LastKnownViewModel recipient, Location message )
        {
            LocationViewModel = new LocationViewModel( message, Logger );

            LocationUrl = $"https://maps.google.com?q={LocationViewModel.Latitude},{LocationViewModel.Longitude}";
        }
    }
}
