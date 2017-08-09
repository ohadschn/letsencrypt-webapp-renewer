using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings
{
    internal class AppSettingsRenewalParamsReader : IAppSettingsRenewalParamsReader
    {
        private readonly IAppSettingsReader m_appSettings;

        public AppSettingsRenewalParamsReader(IAppSettingsReader appSettings)
        {
            m_appSettings = appSettings;
        }

        public IReadOnlyCollection<RenewalParameters> Read()
        {
            Trace.TraceInformation("Parsing Web Apps for SSL renewal from webjob/site configuration...");
            var webApps = m_appSettings.GetDelimitedList("webApps");
            if (webApps.Any(String.IsNullOrWhiteSpace))
            {
                throw new ConfigurationErrorsException("webApp list contains a whitespace-only entry");
            }

            Trace.TraceInformation("Parsed web apps for SSL renewal: {0}", String.Join("; ", webApps));

            var webAppRenewalInfos = webApps.Select(GetWebAppRenewalInfo).ToArray();

            Trace.TraceInformation("Completed parsing of Web App SSL cert renewal information");
            return webAppRenewalInfos;
        }

        private RenewalParameters GetWebAppRenewalInfo(string webApp)
        {
            Trace.TraceInformation("Parsing SSL renewal parameters for web app '{0}'...", webApp);
            try
            {
                return new RenewalParameters(
                    m_appSettings.GetGuid(webApp + "-subscriptionId"),
                    m_appSettings.GetString(webApp + "-tenantId"),
                    m_appSettings.GetString(webApp + "-resourceGroup"),
                    webApp,
                    m_appSettings.GetDelimitedList(webApp + "-hosts"),
                    m_appSettings.GetString(webApp + "-email"),
                    m_appSettings.GetGuid(webApp + "-clientId"),
                    m_appSettings.GetConnectionString(webApp + "-clientSecret"),
                    m_appSettings.GetBooleanOrDefault(webApp + "-useIpBasedSsl"),
                    m_appSettings.GetInt32OrDefault(webApp + "-rsaKeyLength", 2048),
                    m_appSettings.GetUriOrDefault(webApp + "-acmeBasedUri"));
            }
            catch (ArgumentException e)
            {
                throw new ConfigurationErrorsException("Error parsing SSL renewal parameters for web app: " + webApp, e);
            }
        }
    }
}