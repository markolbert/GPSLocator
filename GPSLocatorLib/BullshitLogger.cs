using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.GPSLocator
{
    public interface IBullshitLogger
    {
        void Log(
            string text,
            [ CallerMemberName ] string calledBy = "",
            [ CallerFilePath ] string callerFilePath = "",
            [ CallerLineNumber ] int lineNum = 0
        );

        void Log(
            Exception exception,
            [ CallerMemberName ] string calledBy = "",
            [ CallerFilePath ] string callerFilePath = "",
            [ CallerLineNumber ] int lineNum = 0
        );
    }

}
