using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABI.Windows.System;
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
        private bool _msgTooLong;

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
            var messages = new List<string>();

            // break long messages up into separate ones
            if( _messageToSend!.Length <= 160 )
                messages.Add( _messageToSend );
            else
            {
                var sb = new StringBuilder();

                foreach( var part in _messageToSend.Split( " " ) )
                {
                    var partLength = part.Length;

                    if( partLength + sb.Length + 1 <= 160 )
                    {
                        sb.Append( " " );
                        sb.Append( part );
                    }
                    else
                    {
                        messages.Add(sb.ToString()  );
                        sb.Clear();

                        if( partLength <= 160 )
                            sb.Append( part );
                        else
                        {
                            messages.Add(part[..160]  );
                            sb.Append( part[ 161.. ] );
                        }
                    }
                }
            }

            var success = true;

            for( var idx=0; idx < messages.Count; idx++ )
            {
                var request = new SendMessageRequest(AppViewModel.Configuration, Logger)
                {
                    Text = messages[idx],
                    Sender = ReplyToEmail!
                };

                var response = await ExecuteRequestAsync(request, OnSendMessageRequestStarted, OnSendMessageRequestEnded);

                if( response!.Succeeded )
                    continue;

                AppViewModel.SetStatusMessage($"Couldn't send message #{idx}", StatusMessageType.Important);

                if (response.Error != null)
                    Logger.Error<string>("Invalid configuration, message was '{0}'", response.Error.Description);
                else Logger.Error("Invalid configuration");

                success = false;

                break;
            }

            SendMessageEnabled = true;

            if( success )
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
                MessageTooLong = ( _messageToSend?.Length ?? 0 ) > 0;
            }
        }

        public bool MessageTooLong
        {
            get => _msgTooLong;
            set => SetProperty( ref _msgTooLong, value );
        }

        public string? ReplyToEmail
        {
            get => _replyToEmail;

            set
            {
                SetProperty( ref _replyToEmail, value, true );
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
