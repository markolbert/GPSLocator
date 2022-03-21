using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
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
        private string? _callback;
        private bool _callbackIsValid;
        private bool _sendAllowed = true;
        private bool _msgTooLong;

        public MessagingViewModel( 
            IJ4JLogger logger 
            )
            : base( logger )
        {
            SendMessageCommandAsync = new AsyncRelayCommand( SendMessageHandlerAsync );

            Callback = AppViewModel.Configuration.DefaultCallback;
        }

        protected override bool LocationFilter( Location toCheck ) => toCheck.HasMessage;

        public AsyncRelayCommand SendMessageCommandAsync { get; }

        private async Task SendMessageHandlerAsync()
        {
            if( !ValidateCallback() )
                return;

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
                        messages.Add( sb.ToString() );
                        sb.Clear();

                        if( partLength <= 160 )
                            sb.Append( part );
                        else
                        {
                            messages.Add( part[ ..160 ] );
                            sb.Append( part[ 161.. ] );
                        }
                    }
                }

                if( sb.Length > 0 )
                    messages.Add( sb.ToString() );
            }

            var request = new SendMessageRequest( AppViewModel.Configuration, Logger );

            foreach ( var message in messages )
            {
                request.AddMessage( Callback!, message );
            }

            DeviceResponse<SendMessageCount>? response = null;
            await Task.Run(async () =>
            {
                response = await ExecuteRequestAsync( request, OnSendingMessageStatusChanged );
            });

            if( response!.Succeeded )
            {
                MessageQueue.Default.Message( "Message sent" ).Important().Enqueue();
                MessageQueue.Default.Ready();
            }
            else
            {
                MessageQueue.Default.Message( "Couldn't send message" ).Important().Enqueue();

                if (response.Error?.Description != null)
                    MessageQueue.Default.Message( response.Error.Description).Important().Enqueue();

                MessageQueue.Default.Ready();

                if( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );
            }
        }

        private void OnSendingMessageStatusChanged(DeviceRequestEventArgs args)
        {
            var error = args.Message ?? "Unspecified error";

            (string msg, bool pBar, bool enabled) = args.RequestEvent switch
            {
                RequestEvent.Started => ("Sending message", true, false),
                RequestEvent.Succeeded => ("Message sent", false, true),
                RequestEvent.Aborted => ($"Send failed: {error}", false, true),
                _ => throw new InvalidEnumArgumentException($"Unsupported RequestEvent '{args.RequestEvent}'")
            };

            if( pBar )
                MessageQueue.Default.Message(msg).Indeterminate().Enqueue();
            else MessageQueue.Default.Message(msg).Enqueue();

            RefreshEnabled = enabled;
        }

        private bool ValidateCallback()
        {
            if( string.IsNullOrEmpty( Callback ) )
                return false;

            if( MailAddress.TryCreate( Callback, out _ ) )
                return true;

            if( Callback.All( char.IsDigit ) )
                return Callback.Length >= 10;

            return false;
        }

        public string? MessageToSend
        {
            get => _messageToSend;

            set
            {
                SetProperty( ref _messageToSend, value );

                MessageTooLong = ( _messageToSend?.Length ?? 0 ) > AppViewModel.Configuration.MaxSmsLength;
                OnPropertyChanged(nameof(SendMessageEnabled));
            }
        }

        public bool MessageTooLong
        {
            get => _msgTooLong;
            set => SetProperty( ref _msgTooLong, value );
        }

        public string? Callback
        {
            get => _callback;

            set
            {
                SetProperty( ref _callback, value, true );

                CallbackIsValid = ValidateCallback();
                OnPropertyChanged( nameof( SendMessageEnabled ) );
            }
        }

        public bool CallbackIsValid
        {
            get => _callbackIsValid;

            set
            {
                SetProperty( ref _callbackIsValid, value );
                OnPropertyChanged(nameof(SendMessageEnabled));
            }
        }

        public bool SendMessageEnabled
        {
            get => _sendAllowed && !string.IsNullOrEmpty(_messageToSend) && CallbackIsValid;
        }
    }
}
