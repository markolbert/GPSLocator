using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.InReach
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
