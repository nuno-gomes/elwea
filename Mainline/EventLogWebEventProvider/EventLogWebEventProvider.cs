using System;
using System.Web;
using System.Diagnostics;
using System.Configuration;
using System.Web.Management;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Collections.Specialized;

namespace NG.Web.Management
{
    /// <summary>
    /// Implements an event provider that logs ASP.NET health-monitoring events into the Windows Application Event Log.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed partial class EventLogWebEventProvider : WebEventProvider
    {
        #region Private Constants

        private const int EVENTID_WEBBASEEVENT = 1303;// ??
        private const int EVENTID_WEBMANAGEMENTEVENT = 1304;// ??
        private const int EVENTID_WEBAPPLICATIONLIFETIMEEVENT = 1305;
        private const int EVENTID_WEBREQUESTEVENT = 1306;
        private const int EVENTID_WEBHEARTBEATEVENT = 1307;
        private const int EVENTID_WEBBASEERROREVENT = 1308;// ??
        private const int EVENTID_WEBREQUESTERROREVENT = 1309;
        private const int EVENTID_WEBERROREVENT = 1310;
        private const int EVENTID_WEBAUDITEVENT = 1311;// ??
        private const int EVENTID_WEBSUCCESSAUDITEVENT = 1312;
        private const int EVENTID_WEBAUTHENTICATIONSUCCESSAUDITEVENT = 1313;// ??
        private const int EVENTID_WEBFAILUREAUDITEVENT = 1314;
        private const int EVENTID_WEBAUTHENTICATIONFAILUREAUDITEVENT = 1315;// ??
        private const int EVENTID_WEBVIEWSTATEFAILUREAUDITEVENT = 1316;

        private const int EVENTID_UNKNOWNEVENT = 10001;

        private const int CATEGORY_ASPNET_NONE = 0;
        private const int CATEGORY_ASPNET_SETUP = 1;
        private const int CATEGORY_ASPNET_UNINSTALL = 2;
        private const int CATEGORY_ASPNET_WEBEVENT = 3;
        private const int CATEGORY_ASPNET_FILEMONITORING = 4;
        private const int CATEGORY_ASPNET_ADMINSERVICE = 5;

        private const string EVENTCATEGORY = "Web Event";
        #endregion Private Constants

        #region Private Fields

        private string _logSource;
        private readonly static Dictionary<Type, EntryInfo> _entryInfo;

        #endregion Private Fields

        #region Constructors
        static EventLogWebEventProvider()
        {
            _entryInfo = new Dictionary<Type, EntryInfo>();

            _entryInfo.Add(typeof(global::System.Web.Management.WebBaseEvent), new EntryInfo(EventLogEntryType.Information, EVENTID_WEBBASEEVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebViewStateFailureAuditEvent), new EntryInfo(EventLogEntryType.FailureAudit, EVENTID_WEBVIEWSTATEFAILUREAUDITEVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebAuthenticationSuccessAuditEvent), new EntryInfo(EventLogEntryType.SuccessAudit, EVENTID_WEBAUTHENTICATIONSUCCESSAUDITEVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebAuthenticationFailureAuditEvent), new EntryInfo(EventLogEntryType.FailureAudit, EVENTID_WEBAUTHENTICATIONFAILUREAUDITEVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebFailureAuditEvent), new EntryInfo(EventLogEntryType.FailureAudit, EVENTID_WEBFAILUREAUDITEVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebSuccessAuditEvent), new EntryInfo(EventLogEntryType.SuccessAudit, EVENTID_WEBSUCCESSAUDITEVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebRequestErrorEvent), new EntryInfo(EventLogEntryType.Error, EVENTID_WEBREQUESTERROREVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebErrorEvent), new EntryInfo(EventLogEntryType.Error, EVENTID_WEBERROREVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebApplicationLifetimeEvent), new EntryInfo(EventLogEntryType.Information, EVENTID_WEBAPPLICATIONLIFETIMEEVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebHeartbeatEvent), new EntryInfo(EventLogEntryType.Information, EVENTID_WEBHEARTBEATEVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebRequestEvent), new EntryInfo(EventLogEntryType.Information, EVENTID_WEBREQUESTEVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebBaseErrorEvent), new EntryInfo(EventLogEntryType.Warning, EVENTID_WEBBASEERROREVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebAuditEvent), new EntryInfo(EventLogEntryType.Information, EVENTID_WEBAUDITEVENT));
            _entryInfo.Add(typeof(global::System.Web.Management.WebManagementEvent), new EntryInfo(EventLogEntryType.Information, EVENTID_WEBMANAGEMENTEVENT));
        }
        #endregion Constructors

        #region Private Methods

        private static void CheckUnrecognizedAttributes(NameValueCollection config, string providerName)
        {
            if (config.Count > 0)
            {
                string key = config.GetKey(0);
                if (!string.IsNullOrEmpty(key))
                {
                    throw new ConfigurationErrorsException(string.Format("Unexpected_provider_attribute[{0},{1}]", key, providerName));
                }
            }
        }

        #endregion Private Methods

        #region Overriden Methods

        /// <summary>Moves events from the provider's buffer into the event log.</summary>
        public override void Flush()
        {
        }

        /// <summary>Sets the initial values for this object.</summary>
        /// <param name="config">A <see cref="T:System.Collections.Specialized.NameValueCollection"></see> that specifies the attributes assigned for this provider in the configuration file.</param>
        /// <param name="name">The name used in the configuration file to identify this provider.</param>
        [PreEmptive.Attributes.Setup(CustomEndpoint = "so-s.info/PreEmptive.Web.Services.Messaging/MessagingServiceV2.asmx")]
        [PreEmptive.Attributes.Teardown()]
        [PreEmptive.Attributes.Feature("Active", EventType=PreEmptive.Attributes.FeatureEventTypes.Start)]
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            _logSource = config["source"];
            if (string.IsNullOrEmpty(_logSource))
            {
                throw new ConfigurationErrorsException("EventLogSource cannot be null or empty.");
            }
            config.Remove("source");

            CheckUnrecognizedAttributes(config, name);
        }

        /// <summary>Processes the event passed to the provider.</summary>
        /// <param name="eventRaised">The <see cref="T:System.Web.Management.WebBaseEvent"></see> object to process.</param>
        public override void ProcessEvent(global::System.Web.Management.WebBaseEvent eventRaised)
        {
            Debug.WriteLine(string.Format("{0}[{1}]", eventRaised.GetType().Name, eventRaised.EventCode));

            if (_entryInfo.ContainsKey(eventRaised.GetType()))
            {
                EntryInfo logInfo = _entryInfo[eventRaised.GetType()];
                EventLog.WriteEntry(_logSource, eventRaised.ToFormattedString(), logInfo.LogType, logInfo.EventId, CATEGORY_ASPNET_WEBEVENT);
            }
            else
            {
                EventLog.WriteEntry(_logSource, eventRaised.ToFormattedString(), EventLogEntryType.Information, EVENTID_UNKNOWNEVENT, CATEGORY_ASPNET_WEBEVENT);
            }
        }

        /// <summary>Performs tasks associated with shutting down the provider.</summary>
        [PreEmptive.Attributes.Feature("Active", EventType = PreEmptive.Attributes.FeatureEventTypes.Stop)]
        public override void Shutdown()
        {
        }

        #endregion Overriden Methods
    }
}
