using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace J4JSoftware.InReach
{
    public record ProgressBarMessage( bool Indeterminate )
    {
        public static void EndProgressBar( ProgressBarMessage progressBar ) =>
            WeakReferenceMessenger.Default.Send( new ProgressBarActionMessage( progressBar, ProgressBarAction.Finish ),
                                                 AppConfigViewModel.ResourceNames.ProgressBarMessageToken );
    }

    public record DeterminantProgressBar(int Maximum) : ProgressBarMessage(false);

    public record IndeterminateProgressBar() : ProgressBarMessage(true);

    public enum ProgressBarAction
    {
        Start,
        Pause,
        Finish
    }

    public record ProgressBarActionMessage(ProgressBarMessage Message, ProgressBarAction Action);

    public record ProgressBarIncrementMessage(DeterminantProgressBar State, int Increment);
}
