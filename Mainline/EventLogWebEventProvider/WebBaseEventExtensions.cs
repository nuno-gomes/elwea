using System;
using System.Web.UI;
using System.Globalization;
using System.Web.Management;
using System.Security.Principal;
using System.Collections.Generic;

namespace NG.Web.Management
{
    internal static class WebBaseEventExtensions
    {
        public static string ToFormattedString(this WebBaseEvent eventRaised)
        {
            List<string> data = new List<string>();

            data.Add(eventRaised.ToString());

            if (eventRaised is WebManagementEvent)
            {
                AddWebProcessInformation(data, ((WebManagementEvent)eventRaised).ProcessInformation);
            }
            if (eventRaised is WebHeartbeatEvent)
            {
                AddWebProcessStatistics(data, ((WebHeartbeatEvent)eventRaised).ProcessStatistics);
            }
            if (eventRaised is WebRequestEvent)
            {
                AddWebRequestInformation(data, ((WebRequestEvent)eventRaised).RequestInformation);
            }
            if (eventRaised is WebBaseErrorEvent)
            {
                AddException(data, ((WebBaseErrorEvent)eventRaised).ErrorException);
            }
            if (eventRaised is WebAuditEvent)
            {
                AddWebRequestInformation(data, ((WebAuditEvent)eventRaised).RequestInformation);
            }
            if (eventRaised is WebRequestErrorEvent)
            {
                AddWebRequestInformation(data, ((WebRequestErrorEvent)eventRaised).RequestInformation);
                AddWebThreadInformation(data, ((WebRequestErrorEvent)eventRaised).ThreadInformation);
            }
            if (eventRaised is WebErrorEvent)
            {
                AddWebRequestInformation(data, ((WebErrorEvent)eventRaised).RequestInformation);
                AddWebThreadInformation(data, ((WebErrorEvent)eventRaised).ThreadInformation);
            }
            if (eventRaised is WebAuthenticationSuccessAuditEvent)
            {
                data.Add(((WebAuthenticationSuccessAuditEvent)eventRaised).NameToAuthenticate);
            }
            if (eventRaised is WebAuthenticationFailureAuditEvent)
            {
                data.Add(((WebAuthenticationFailureAuditEvent)eventRaised).NameToAuthenticate);
            }
            if (eventRaised is WebViewStateFailureAuditEvent)
            {
                AddViewStateException(data, ((WebViewStateFailureAuditEvent)eventRaised).ViewStateException);
            }

            return string.Join<string>("\n", data);
        }
        
        private static void AddException(IList<string> data, Exception exception)
        {
            if (exception == null)
            {
                data.Add(null);
                data.Add(null);
            }
            else
            {
                data.Add(exception.GetType().Name);
                data.Add(exception.Message);
            }
        }

        private static void AddViewStateException(IList<string> data, ViewStateException vse)
        {
            //dataFields.Add(SR.GetString(vse.ShortMessage));
            data.Add("ViewState_InvalidViewState");
            data.Add(vse.RemoteAddress);
            data.Add(vse.RemotePort);
            data.Add(vse.UserAgent);
            data.Add(vse.PersistedState);
            data.Add(vse.Referer);
            data.Add(vse.Path);
        }

        private static void AddWebProcessInformation(IList<string> data, WebProcessInformation processEventInfo)
        {
            data.Add(processEventInfo.ProcessID.ToString(CultureInfo.InstalledUICulture));
            data.Add(processEventInfo.ProcessName);
            data.Add(processEventInfo.AccountName);
        }

        private static void AddWebProcessStatistics(IList<string> data, WebProcessStatistics procStats)
        {
            data.Add(procStats.ProcessStartTime.ToString(CultureInfo.InstalledUICulture));
            data.Add(procStats.ThreadCount.ToString(CultureInfo.InstalledUICulture));
            data.Add(procStats.WorkingSet.ToString(CultureInfo.InstalledUICulture));
            data.Add(procStats.PeakWorkingSet.ToString(CultureInfo.InstalledUICulture));
            data.Add(procStats.ManagedHeapSize.ToString(CultureInfo.InstalledUICulture));
            data.Add(procStats.AppDomainCount.ToString(CultureInfo.InstalledUICulture));
            data.Add(procStats.RequestsExecuting.ToString(CultureInfo.InstalledUICulture));
            data.Add(procStats.RequestsQueued.ToString(CultureInfo.InstalledUICulture));
            data.Add(procStats.RequestsRejected.ToString(CultureInfo.InstalledUICulture));
        }

        private static void AddWebRequestInformation(IList<string> data, WebRequestInformation reqInfo)
        {
            string name;
            string authenticationType;
            bool isAuthenticated;
            IPrincipal principal = reqInfo.Principal;
            if (principal == null)
            {
                name = null;
                isAuthenticated = false;
                authenticationType = null;
            }
            else
            {
                IIdentity identity = principal.Identity;
                name = identity.Name;
                isAuthenticated = identity.IsAuthenticated;
                authenticationType = identity.AuthenticationType;
            }
            data.Add(reqInfo.RequestUrl);
            data.Add(reqInfo.RequestPath);
            data.Add(reqInfo.UserHostAddress);
            data.Add(name);
            data.Add(isAuthenticated.ToString());
            data.Add(authenticationType);
            data.Add(reqInfo.ThreadAccountName);
        }

        private static void AddWebThreadInformation(IList<string> data, WebThreadInformation threadInfo)
        {
            data.Add(threadInfo.ThreadID.ToString(CultureInfo.InstalledUICulture));
            data.Add(threadInfo.ThreadAccountName);
            data.Add(threadInfo.IsImpersonating.ToString(CultureInfo.InstalledUICulture));
            data.Add(threadInfo.StackTrace);
        }

    }
}
