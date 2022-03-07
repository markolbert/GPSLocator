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
        #region Collection

        public class Choices : IEnumerable<AnnotatedLocationType>
        {
            private readonly List<AnnotatedLocationType> _values = new();

            public Choices()
            {
                _values.Add(new AnnotatedLocationType(this, LocationType.LinePoint, "Include in Route"));
                _values.Add(new AnnotatedLocationType(this, LocationType.Pushpin, "Show as Pushpin"));
                _values.Add(new AnnotatedLocationType(this, LocationType.Unspecified, "Don't Show"));
            }

            public IEnumerator<AnnotatedLocationType> GetEnumerator()
            {
                foreach( var item in _values )
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        #endregion

        private readonly AnnotatedLocationType.Choices _collection;

        private AnnotatedLocationType(
            AnnotatedLocationType.Choices collection,
            LocationType locationType,
            string? label = null
        )
        {
            _collection = collection;

            LocationType = locationType;
            Label = label ?? locationType.ToString();
        }

        public LocationType LocationType { get; }
        public string Label { get; }
    }
}
