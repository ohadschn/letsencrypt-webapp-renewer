using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob
{
    public class Renewer : IRenewer
    {
        public void RenewWebAppCertFromConfiguration(IRenewalManager renewalManager, IConfigurationHelper configHelper)
        {
            if (configHelper == null)
            {
                throw new ArgumentNullException(nameof(configHelper));
            }

            Trace.TraceInformation("Parsing Web Apps for SSL renewal from webjob/site configuration...");
            var webApps = configHelper.GetDelimitedList("webApps");
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
                webAppRenewalInfos[i] = GetWebAppRenewalInfo(configHelper, webApp);
            }

            Parallel.For(0, webApps.Count, i =>
            {
                var webAppRenewalInfo = webAppRenewalInfos[i];
                Trace.TraceInformation("Renewing SSL cert for Web App '{0}'...", webAppRenewalInfo.WebApp);
                renewalManager.Renew(webAppRenewalInfo);
                Trace.TraceInformation("Completed renewal of SSL cert for Web App '{0}'", webAppRenewalInfo.WebApp);
            });
        }

        private static RenewalParameters GetWebAppRenewalInfo(IConfigurationHelper configHelper, string webApp)
        {
            return new RenewalParameters(
                configHelper.GetString(webApp + "-tenantId"),
                configHelper.GetGuid(webApp + "-subscriptionId"),
                configHelper.GetGuid(webApp + "-clientId"),
                configHelper.GetConnectionString(webApp + "-clientSecret"),
                configHelper.GetString(webApp + "-resourceGroup"),
                webApp,
                configHelper.GetString(webApp + "-email"),
                configHelper.GetDelimitedList(webApp + "-hosts"),
                configHelper.GetBooleanOrDefault(webApp + "-useIpBasedSsl"),
                configHelper.GetInt32OrDefault(webApp + "-rsaKeyLength", 2048),
                configHelper.GetUriOrDefault(webApp + "-acmeBasedUri"));
        }
    }
}