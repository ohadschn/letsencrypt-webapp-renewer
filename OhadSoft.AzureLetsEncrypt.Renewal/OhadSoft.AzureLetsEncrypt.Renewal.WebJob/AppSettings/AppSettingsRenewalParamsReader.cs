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

            var subscriptionIdKey = webApp + "-subscriptionId";
            var tenantIdKey = webApp + "-tenantId";
            var resourceGroupKey = webApp + "-resourceGroup";
            var hostsKey = webApp + "-hosts";
            var emailKey = webApp + "-email";
            var clientIdKey = webApp + "-clientId";
            var clientSecretKey = webApp + "-clientSecret";
            var useIpBasedSslKey = webApp + "-useIpBasedSsl";
            var rsaKeyLengthKey = webApp + "-rsaKeyLength";
            var acmeBaseUri = webApp + "-acmeBaseUri";

            try
            {
                // ReSharper disable once SimplifyConditionalTernaryExpression
                return new RenewalParameters(
                    m_appSettings.GetGuid(subscriptionIdKey),
                    m_appSettings.GetString(tenantIdKey),
                    m_appSettings.GetString(resourceGroupKey),
                    webApp,
                    m_appSettings.GetDelimitedList(hostsKey),
                    m_appSettings.GetString(emailKey),
                    m_appSettings.GetGuid(clientIdKey),
                    m_appSettings.GetConnectionString(clientSecretKey),
                    m_appSettings.HasSetting(useIpBasedSslKey) ? m_appSettings.GetBoolean(useIpBasedSslKey) : false,
                    m_appSettings.HasSetting(rsaKeyLengthKey) ? m_appSettings.GetInt32(rsaKeyLengthKey) : 2048,
                    m_appSettings.HasSetting(acmeBaseUri) ? m_appSettings.GetUri(acmeBaseUri) : null);
            }
            catch (ArgumentException e)
            {
                throw new ConfigurationErrorsException("Error parsing SSL renewal parameters for web app: " + webApp, e);
            }
        }
    }
}