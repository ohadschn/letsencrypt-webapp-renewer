using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LetsEncrypt.Azure.Core;
using LetsEncrypt.Azure.Core.Models;
using OhadSoft.AzureLetsEncrypt.Renewal.Configuration;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public class RenewalManager : IRenewalManager
    {
        [SuppressMessage("Sonar", "S1075:URIsShouldNotBeHardcoded", Justification = "Default URI")]
        public const string DefaultAcmeBaseUri = "https://acme-v01.api.letsencrypt.org/";

        private static readonly RNGCryptoServiceProvider s_randomGenerator = new RNGCryptoServiceProvider(); // thread-safe

        public async Task Renew(RenewalParameters renewalParams)
        {
            if (renewalParams == null)
            {
                throw new ArgumentNullException(nameof(renewalParams));
            }

            Trace.TraceInformation("Generating SSL certificate with parameters: {0}", renewalParams);

            Trace.TraceInformation("Generating secure PFX password for '{0}'...", renewalParams.WebApp);
            var pfxPassData = new byte[32];
            s_randomGenerator.GetBytes(pfxPassData);

            Trace.TraceInformation("Adding SSL cert for '{0}'...", renewalParams.WebApp);

            var manager = CertificateManager.CreateKuduWebAppCertificateManager(
                new AzureWebAppEnvironment(
                    renewalParams.TenantId,
                    renewalParams.SubscriptionId,
                    renewalParams.ClientId,
                    renewalParams.ClientSecret,
                    renewalParams.ResourceGroup,
                    renewalParams.WebApp,
                    renewalParams.ServicePlanResourceGroup,
                    renewalParams.SiteSlotName),
                new AcmeConfig
                {
                    Host = renewalParams.Hosts[0],
                    AlternateNames = renewalParams.Hosts.Skip(1).ToList(),
                    RegistrationEmail = renewalParams.Email,
                    RSAKeyLength = renewalParams.RsaKeyLength,
                    PFXPassword = Convert.ToBase64String(pfxPassData),
#pragma warning disable S1075
                    BaseUri = (renewalParams.AcmeBaseUri ?? new Uri(DefaultAcmeBaseUri)).ToString()
#pragma warning restore S1075
                },
                new CertificateServiceSettings { UseIPBasedSSL = renewalParams.UseIpBasedSsl },
                new AuthProviderConfig());

            if (renewalParams.RenewXNumberOfDaysBeforeExpiration > 0)
            {
                await manager.RenewCertificate(false, renewalParams.RenewXNumberOfDaysBeforeExpiration);
            }
            else
            {
                await manager.AddCertificate();
            }

            Trace.TraceInformation("SSL cert added successfully to '{0}'", renewalParams.WebApp);
        }
    }
}