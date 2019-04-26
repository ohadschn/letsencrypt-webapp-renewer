using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LetsEncrypt.Azure.Core;
using LetsEncrypt.Azure.Core.Models;
using Microsoft.Azure.Management.WebSites.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Util
{
    public static class CertificateHelper
    {
        // https://github.com/sjkp/letsencrypt-siteextension/blob/8e758579b21b0dac5269337e30ac88b629818889/LetsEncrypt.SiteExtension.Core/CertificateManager.cs#L146
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Task")]
        public static async Task<IReadOnlyList<string>> GetLetsEncryptHostNames(IAzureWebAppEnvironment webAppEnvironment, bool staging)
        {
            Site site;
            using (var client = await ArmHelper.GetWebSiteManagementClient(webAppEnvironment))
            {
                Trace.TraceInformation(
                    "Getting Web App '{0}' (slot '{1}') from resource group '{2}'",
                    webAppEnvironment.WebAppName,
                    webAppEnvironment.SiteSlotName,
                    webAppEnvironment.ResourceGroupName);

                site = client.WebApps.GetSiteOrSlot(webAppEnvironment.ResourceGroupName, webAppEnvironment.WebAppName, webAppEnvironment.SiteSlotName);
            }

            using (var httpClient = await ArmHelper.GetHttpClient(webAppEnvironment))
            {
                var certRequestUri = $"/subscriptions/{webAppEnvironment.SubscriptionId}/providers/Microsoft.Web/certificates?api-version=2016-03-01";
                Trace.TraceInformation("GET {0}", certRequestUri);
                var response = await ArmHelper.ExponentialBackoff().ExecuteAsync(async () => await httpClient.GetAsync(certRequestUri));

                Trace.TraceInformation("Reading ARM certificate query response");
                var body = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

                var letsEncryptCerts = ExtractCertificates(body).Where(s => s.Issuer.Contains(staging ? "Fake LE" : "Let's Encrypt"));

                var leCertThumbprints = new HashSet<string>(letsEncryptCerts.Select(c => c.Thumbprint));
                return site.HostNameSslStates.Where(ssl => leCertThumbprints.Contains(ssl.Thumbprint)).Select(ssl => ssl.Name).ToArray();
            }
        }

        // https://github.com/sjkp/letsencrypt-siteextension/blob/8e758579b21b0dac5269337e30ac88b629818889/LetsEncrypt.SiteExtension.Core/CertificateManager.cs#L204
        internal static IEnumerable<Certificate> ExtractCertificates(string body)
        {
            Trace.TraceInformation("Deserializing certificates from ARM response");

            var json = JToken.Parse(body);
            return json.Type == JTokenType.Object && json["value"] != null
                ? JsonConvert.DeserializeObject<Certificate[]>(json["value"].ToString(), JsonHelper.DefaultSerializationSettings)
                : JsonConvert.DeserializeObject<Certificate[]>(body, JsonHelper.DefaultSerializationSettings);
        }
    }
}