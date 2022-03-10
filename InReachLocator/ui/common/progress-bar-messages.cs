using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.InReach
{
    public record ProgressBarState(bool Indeterminate);

    public record DeterminantProgressBar(int Maximum) : ProgressBarState(false);

    public record IndeterminateProgressBar() : ProgressBarState(true);

    public enum ProgressBarMessageType
    {
        Start,
        Pause,
        Finish
    }

    public record ProgressBarActionMessage(ProgressBarState State, ProgressBarMessageType MessageType);

    public record ProgressBarIncrementMessage(DeterminantProgressBar State, int Increment);
}
