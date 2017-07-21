using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OhadSoft.AzureLetsEncrypt.Renewal.Configuration;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public class Renewer : IRenewer
    {
        public void RenewWebAppCertFromConfiguration(IRenewalManager renewalManager, IConfigurationReader configReader)
        {
            if (configReader == null)
            {
                throw new ArgumentNullException(nameof(configReader));
            }

            Trace.TraceInformation("Parsing Web Apps for SSL renewal from webjob/site configuration...");
            var webApps = configReader.GetDelimitedList("webApps");
            if (webApps.Any(String.IsNullOrWhiteSpace))
            {
                throw new ConfigurationErrorsException("webApp list contains a whitespace-only entry");
            }

            Trace.TraceInformation("Parsed web apps for SSL renewal: {0}", String.Join("; ", webApps));

            var webAppRenewalInfos = new RenewalParameters[webApps.Count];
            for (int i = 0; i < webApps.Count; i++)
            {
                var webApp = webApps[i];
                Trace.TraceInformation("Parsing SSL renewal parameters for web app '{0}'...", webApp);
                webAppRenewalInfos[i] = GetWebAppRenewalInfo(configReader, webApp);
            }

            Parallel.For(0, webApps.Count, i =>
            {
                var webAppRenewalInfo = webAppRenewalInfos[i];
                Trace.TraceInformation("Renewing SSL cert for Web App '{0}'...", webAppRenewalInfo.WebApp);
                renewalManager.Renew(webAppRenewalInfo);
                Trace.TraceInformation("Completed renewal of SSL cert for Web App '{0}'", webAppRenewalInfo.WebApp);
            });
        }

        private static RenewalParameters GetWebAppRenewalInfo(IConfigurationReader configReader, string webApp)
        {
            return new RenewalParameters(
                configReader.GetString(webApp + "-tenantId"),
                configReader.GetGuid(webApp + "-subscriptionId"),
                configReader.GetGuid(webApp + "-clientId"),
                configReader.GetConnectionString(webApp + "-clientSecret"),
                configReader.GetString(webApp + "-resourceGroup"),
                webApp,
                configReader.GetString(webApp + "-email"),
                configReader.GetDelimitedList(webApp + "-hosts"),
                configReader.GetBooleanOrDefault(webApp + "-useIpBasedSsl"),
                configReader.GetInt32OrDefault(webApp + "-rsaKeyLength", 2048),
                configReader.GetUriOrDefault(webApp + "-acmeBasedUri"));
        }
    }
}