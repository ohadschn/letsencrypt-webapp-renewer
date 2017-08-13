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
        public const string KeyPrefix = "letsencrypt:";
        private readonly IAppSettingsReader m_appSettings;

        public AppSettingsRenewalParamsReader(IAppSettingsReader appSettings)
        {
            m_appSettings = appSettings;
        }

        public IReadOnlyCollection<RenewalParameters> Read()
        {
            Trace.TraceInformation("Parsing Web Apps for SSL renewal from webjob/site configuration...");
            var webApps = m_appSettings.GetDelimitedList(KeyPrefix + "webApps");
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

            var subscriptionIdKey = KeyPrefix + webApp + "-subscriptionId";
            var tenantIdKey = KeyPrefix + webApp + "-tenantId";
            var resourceGroupKey = KeyPrefix + webApp + "-resourceGroup";
            var hostsKey = KeyPrefix + webApp + "-hosts";
            var emailKey = KeyPrefix + webApp + "-email";
            var clientIdKey = KeyPrefix + webApp + "-clientId";
            var clientSecretKey = KeyPrefix + webApp + "-clientSecret";
            var useIpBasedSslKey = KeyPrefix + webApp + "-useIpBasedSsl";
            var rsaKeyLengthKey = KeyPrefix + webApp + "-rsaKeyLength";
            var acmeBaseUri = KeyPrefix + webApp + "-acmeBaseUri";

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