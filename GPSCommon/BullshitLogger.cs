using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.GPSLocator;

namespace J4JSoftware.GPSCommon;

public class BullshitLogger : IBullshitLogger
{
    private readonly string _logFile;

    public BullshitLogger()
    {
        _logFile = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "bullshit.txt");
    }

    public void Log(string text, [CallerMemberName] string calledBy = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0)
    {
        File.AppendAllText(_logFile, $"{calledBy}\t{text}\n\t{callerFilePath}:{lineNum}\n");
    }
}