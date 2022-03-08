using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.InReach.Annotations;

namespace J4JSoftware.InReach
{
    public class AppConfig : InReachConfig, IAppConfig
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isValid;

        public bool IsValid
        {
            get=> _isValid;

            set
            {
                var changed = value != _isValid;
                _isValid = value;

                if( changed )
                    OnPropertyChanged( nameof( IsValid ) );
            }
        }

        [ NotifyPropertyChangedInvocator ]
        protected virtual void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
