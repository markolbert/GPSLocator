using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GPSLocator;

public class MessagingViewModel :SelectablePointViewModel
{
    private string? _messageToSend;
    private string? _callback;
    private bool _callbackIsValid;
    private bool _sendAllowed = true;
    private bool _msgTooLong;

    public MessagingViewModel(
        AppViewModel appViewModel,
        IJ4JLogger logger 
    )
        : base( appViewModel, logger )
    {
        SendMessageCommand = new RelayCommand( SendMessageHandler );

        Callback = AppViewModel.Configuration.DefaultCallback;
    }

    protected override bool LocationFilter( Location toCheck ) => toCheck.HasMessage;

    public RelayCommand SendMessageCommand { get; }

    private void SendMessageHandler()
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

        foreach( var message in messages )
        {
            request.AddMessage( Callback!, message );
        }

        ExecuteRequest(request, OnSendingMessageStatusChanged);
    }

    private void OnSendingMessageStatusChanged(RequestEventArgs<SendMessageCount> args)
    {
        switch (args.RequestEvent)
        {
            case RequestEvent.Started:
                StatusMessages.Message("Sending message").Indeterminate().Important().Display();
                RefreshEnabled = false;

                break;

            case RequestEvent.Succeeded:
                OnSucceeded(args);
                break;

            case RequestEvent.Aborted:
                OnAborted(args);
                break;

            default:
                throw new InvalidEnumArgumentException($"Unsupported {typeof(RequestEvent)} '{args.RequestEvent}'");
        }
    }

    private void OnSucceeded(RequestEventArgs<SendMessageCount> args)
    {
        StatusMessages.Message("Message sent").Important().Enqueue();
        StatusMessages.DisplayReady();

        RefreshEnabled = true;
    }

    private void OnAborted(RequestEventArgs<SendMessageCount> args)
    {
        StatusMessages.Message($"Send failed ({(args.ErrorMessage ?? "Unspecified error")})")
                      .Important()
                      .Enqueue();

        StatusMessages.DisplayReady();

        if (args.Response?.Error != null)
            Logger.Error<string>("Invalid configuration, message was '{0}'", args.Response.Error.Description);
        else Logger.Error("Invalid configuration");

        RefreshEnabled = true;
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