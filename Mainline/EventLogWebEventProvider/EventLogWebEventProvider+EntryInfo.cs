using System;
using System.Diagnostics;

namespace NG.Web.Management
{
    public partial class EventLogWebEventProvider
    {
        private class EntryInfo
        {
            public EntryInfo(EventLogEntryType logType, int eventId)
            {
                this.LogType = logType;
                this.EventId = eventId;
            }

            public EventLogEntryType LogType
            {
                get;
                private set;
            }

            public int EventId
            {
                get;
                private set;
            }
        }
    }
}
