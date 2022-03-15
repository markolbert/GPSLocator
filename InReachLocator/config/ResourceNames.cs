using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.InReach
{
    public class ResourceNames
    {
        public string HistoryPageName => HistoryPage.PageName;
        public string LastKnownPageName => LastKnownPage.PageName;
        public string LogViewerPageName => LogViewerPage.PageName;
        public string HelpTag => "Help";
        public string HelpLink => "https://github.com/markolbert/InReach/blob/main/docs/main.md";

        public string NormalStyleKey => "NormalStatusMessageStyle";
        public string ImportantStyleKey => "ImportantStatusMessageStyle";
        public string UrgentStyleKey => "UrgentStatusMessageStyle";

        public string StatusMessageToken => "StatusMessage";
        public string ProgressBarMessageToken => "ProgressBarMessage";
        public string LocationTypeChangedMessageToken => "LocationTypeChanged";
    }
}
