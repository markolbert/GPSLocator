using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.InReach
{
    public enum StatusMessageType
    {
        Normal,
        Important,
        Urgent
    }

    public class StatusMessage : ObservableObject
    {
        public static void Send( string mesg, StatusMessageType type = StatusMessageType.Normal )
        {
            WeakReferenceMessenger.Default.Send( new StatusMessage() { Message = mesg, Type = type },
                                                 AppConfig.ResourceNames.StatusMessageToken );
        }

        private static readonly object NormalStyle;
        private static readonly object ImportantStyle;
        private static readonly object UrgentStyle;

        static StatusMessage()
        {
            NormalStyle = GetStyle( AppConfig.ResourceNames.NormalStyleKey );
            ImportantStyle = GetStyle( AppConfig.ResourceNames.ImportantStyleKey );
            UrgentStyle = GetStyle( AppConfig.ResourceNames.UrgentStyleKey );

            object GetStyle( string key ) =>
                App.Current.Resources.ContainsKey( key )
                    ? App.Current.Resources[ key ]
                    : DependencyProperty.UnsetValue;
        }

        private string? _message;
        private StatusMessageType _type;

        public string? Message
        {
            get => _message;
            set => SetProperty( ref _message, value );
        }

        public StatusMessageType Type
        {
            get => _type;

            set
            {
                SetProperty( ref _type, value );
                OnPropertyChanged(nameof(MessageStyle));
            }
        }

        public object MessageStyle =>
            _type switch
            {
                StatusMessageType.Normal => NormalStyle,
                StatusMessageType.Important => ImportantStyle,
                StatusMessageType.Urgent => UrgentStyle,
                _ => DependencyProperty.UnsetValue
            };
    }
}
