namespace J4JSoftware.GPSLocator;

public class ResourceNames
{
    public string HistoryPageName => HistoryPage.PageName;
    public string LastKnownPageName => LastKnownPage.PageName;
    public string LogViewerPageName => LogViewerPage.PageName;
    public string MessagingPageName => MessagingPage.PageName;
    public string SettingsPageName => SettingsPage.PageName;
    public string AboutPageName => AboutPage.PageName;
    public static string NullPageName => "-- none --";

    public string HelpTag => "Help";

    public string OpenMapCopyrightUri => "https://www.openstreetmap.org/copyright";

    public string NormalStyleKey => "NormalStatusMessageStyle";
    public string ImportantStyleKey => "ImportantStatusMessageStyle";
    public string UrgentStyleKey => "UrgentStatusMessageStyle";

    public string StatusMessageToken => "StatusMessage";
    public string ProgressBarMessageToken => "ProgressBarMessage";
    public string LocationTypeChangedMessageToken => "LocationTypeChanged";
}