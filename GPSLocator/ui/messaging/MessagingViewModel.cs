using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
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
                response = await ExecuteRequestAsync( request, OnSendMessageRequestStarted, OnSendMessageRequestEnded );
            });

            if( response!.Succeeded )
                await AppViewModel.SetStatusMessagesAsync( 1000,
                                                           new StatusMessage( "Message sent",
                                                                              StatusMessageType.Important ),
                                                           new StatusMessage( "Ready" ) );
            else
            {
                await AppViewModel.SetStatusMessagesAsync( 2000,
                                                           new StatusMessage( $"Couldn't send message",
                                                                              StatusMessageType.Important ),
                                                           new StatusMessage( "Ready" ) );


                if( response.Error != null )
                    Logger.Error<string>( "Invalid configuration, message was '{0}'", response.Error.Description );
                else Logger.Error( "Invalid configuration" );
            }
        }

        private void OnSendMessageRequestStarted()
        {
            AppViewModel.SetStatusMessage("Sending message");
            AppViewModel.IndeterminateVisibility = Visibility.Visible;

            _sendAllowed = true;
            OnPropertyChanged(nameof(SendMessageEnabled));
        }

        private void OnSendMessageRequestEnded()
        {
            AppViewModel.IndeterminateVisibility = Visibility.Collapsed;

            _sendAllowed = true;
            OnPropertyChanged(nameof(SendMessageEnabled));
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
