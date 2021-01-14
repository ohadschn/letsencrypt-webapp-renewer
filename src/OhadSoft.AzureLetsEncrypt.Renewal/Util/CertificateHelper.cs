using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LetsEncrypt.Azure.Core;
using LetsEncrypt.Azure.Core.Models;
using Microsoft.Azure.Management.WebSites.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Util
{
    internal static class CertificateHelper
    {
        internal static string[] LetsEncryptIssuerNames = new[] { "Let's Encrypt Authority X1", "Let's Encrypt Authority X2", "Let's Encrypt Authority X3", "Let's Encrypt Authority X4", "R3", "R4", "E1", "E2" };
        internal static string[] LetsEncrypStagingtIssuerNames = new[] { "Fake LE Intermediate X1" };

        private static readonly RNGCryptoServiceProvider s_randomGenerator = new RNGCryptoServiceProvider(); // thread-safe

        // https://github.com/sjkp/letsencrypt-siteextension/blob/8e758579b21b0dac5269337e30ac88b629818889/LetsEncrypt.SiteExtension.Core/CertificateManager.cs#L146
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Task")]
        public static async Task<IReadOnlyList<string>> GetLetsEncryptHostNames(IAzureWebAppEnvironment webAppEnvironment, bool staging)
        {
            Site site;
            using (var client = await ArmHelper.GetWebSiteManagementClient(webAppEnvironment).ConfigureAwait(false))
            {
                var webAppName = webAppEnvironment.WebAppName;
                var resourceGroupName = webAppEnvironment.ResourceGroupName;
                var siteSlotName = webAppEnvironment.SiteSlotName;
                Trace.TraceInformation(
                    "Getting Web App '{0}' (slot '{1}') from resource group '{2}'",
                    webAppName,
                    siteSlotName,
                    resourceGroupName);

                site = client.WebApps.GetSiteOrSlot(resourceGroupName, webAppName, siteSlotName);
                if (site == null)
                {
                    throw new InvalidOperationException(
                        $"Could not find web app '{webAppName}' (slot '{siteSlotName}') under Resource Groups '{resourceGroupName}'");
                }
            }

            using (var httpClient = await ArmHelper.GetHttpClient(webAppEnvironment).ConfigureAwait(false))
            {
                var certRequestUri = $"/subscriptions/{webAppEnvironment.SubscriptionId}/providers/Microsoft.Web/certificates?api-version=2016-03-01";
                Trace.TraceInformation("GET {0}", certRequestUri);
                var response = await ArmHelper.ExponentialBackoff().ExecuteAsync(() => httpClient.GetAsync(new Uri(certRequestUri, UriKind.Relative))).ConfigureAwait(false);

                Trace.TraceInformation("Reading ARM certificate query response");
                var body = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().ConfigureAwait(false);

                var letsEncryptIssuerNames = staging ? LetsEncrypStagingtIssuerNames : LetsEncryptIssuerNames;
                var letsEncryptCerts = ExtractCertificates(body).Where(cert => letsEncryptIssuerNames.Contains(cert.Issuer));

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

        public static string GenerateSecurePassword()
        {
            var pfxPassData = new byte[32];
            s_randomGenerator.GetBytes(pfxPassData);
            return Convert.ToBase64String(pfxPassData);
        }
    }
}