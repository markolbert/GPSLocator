using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace J4JSoftware.GPSLocator
{
    public class MessagingViewModel :HistoryViewModelBase
    {
        private string? _messageToSend;
        private string? _replyToEmail;
        private bool _sendMessageEnabled;

        public MessagingViewModel( 
            IJ4JLogger logger 
            )
            : base( logger )
        {
            SendMessageCommandAsync = new AsyncRelayCommand( SendMessageHandlerAsync );
        }

        protected override bool LocationFilter( Location toCheck ) => toCheck.HasMessage;

        public AsyncRelayCommand SendMessageCommandAsync { get; }

        private async Task SendMessageHandlerAsync()
        {
            var request = new SendMessageRequest( AppViewModel.Configuration, Logger )
            {
                Text = MessageToSend!, Sender = ReplyToEmail!
            };

            var response = await ExecuteRequestAsync( request, OnSendMessageRequestStarted, OnSendMessageRequestEnded );

            if( !response!.Succeeded )
            {
                AppViewModel.SetStatusMessage( "Couldn't send message", StatusMessageType.Important );

                if( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );

                SendMessageEnabled = true;

                return;
            }

            SendMessageEnabled = true;
            AppViewModel.SetStatusMessage( "Ready" );
        }

        private void OnSendMessageRequestStarted()
        {
            AppViewModel.SetStatusMessage("Sending message");
            AppViewModel.IndeterminateVisibility = Visibility.Visible;
            SendMessageEnabled = false;
        }

        private void OnSendMessageRequestEnded()
        {
            AppViewModel.IndeterminateVisibility = Visibility.Collapsed;
            SendMessageEnabled = true;
        }

        public string? MessageToSend
        {
            get => _messageToSend;

            set
            {
                SetProperty( ref _messageToSend, value );
                SendMessageEnabled = !string.IsNullOrEmpty( _messageToSend ) && !string.IsNullOrEmpty( _replyToEmail );
            }
        }

        public string? ReplyToEmail
        {
            get => _replyToEmail;

            set
            {
                SetProperty( ref _replyToEmail, value );
                SendMessageEnabled = !string.IsNullOrEmpty( _messageToSend ) && !string.IsNullOrEmpty( _replyToEmail );
            }
        }

        public bool SendMessageEnabled
        {
            get => _sendMessageEnabled;
            set => SetProperty( ref _sendMessageEnabled, value );
        }
    }
}
