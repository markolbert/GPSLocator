using System.Collections.Generic;

namespace J4JSoftware.GPSLocator;

public class NavigationTargets
{
    public static List<SelectablePage> Pages { get; } = new()
    {
        new SelectablePage(null, ResourceNames.NullPageName),
        new SelectablePage(typeof(LastKnownPage),
                           ResourceNames.LastKnownPageName,
                           "Last Known Location"),
        new SelectablePage(typeof(HistoryPage), ResourceNames.HistoryPageName, "History"),
        new SelectablePage(typeof(MessagingPage), ResourceNames.MessagingPageName, "Messaging"),
        new SelectablePage(typeof(AboutPage), ResourceNames.AboutPageName, "About"),
        new SelectablePage(typeof(LogViewerPage), ResourceNames.LogViewerPageName, "Log Viewer"),
        new SelectablePage(typeof(SettingsPage), ResourceNames.SettingsPageName, "Settings")
    };
}
