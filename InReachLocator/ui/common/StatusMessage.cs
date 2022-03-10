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
        public const string NormalStyleKey = "NormalStatusMessageStyle";
        public const string ImportantStyleKey = "ImportantStatusMessageStyle";
        public const string UrgentStyleKey = "UrgentStatusMessageStyle";

        public const string StatusMessageToken = "statusmessage";

        public static void Send( string mesg, StatusMessageType type = StatusMessageType.Normal ) =>
            WeakReferenceMessenger.Default.Send( new StatusMessage() { Message = mesg, Type = type }, StatusMessageToken );

        private static readonly object NormalStyle;
        private static readonly object ImportantStyle;
        private static readonly object UrgentStyle;

        static StatusMessage()
        {
            NormalStyle = App.Current.Resources.ContainsKey(NormalStyleKey)
                ? App.Current.Resources[NormalStyleKey]
                : DependencyProperty.UnsetValue;

            ImportantStyle = App.Current.Resources.ContainsKey(ImportantStyleKey)
                ? App.Current.Resources[ImportantStyleKey]
                : DependencyProperty.UnsetValue;

            UrgentStyle = App.Current.Resources.ContainsKey(UrgentStyleKey)
                ? App.Current.Resources[UrgentStyleKey]
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
