using System;
using System.Configuration;
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

            var webApps = configHelper.GetDelimitedList("webApps");
            if (webApps.Any(String.IsNullOrWhiteSpace))
            {
                throw new ConfigurationErrorsException("webApp list contains a whitespace-only entry");    
            }

            var webAppRenewalInfos = new RenewalParameters[webApps.Count];

            for (int i = 0; i < webApps.Count; i++)
            {
                var webApp = webApps[i];
                webAppRenewalInfos[i] = new RenewalParameters(
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

            Parallel.For(0, webApps.Count, i =>
            {
                renewalManager.Renew(webAppRenewalInfos[i]);
            });
        }
    }
}