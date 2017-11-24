using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LetsEncrypt.Azure.Core;
using LetsEncrypt.Azure.Core.Models;
using LetsEncrypt.Azure.Core.Services;
using OhadSoft.AzureLetsEncrypt.Renewal.Configuration;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public class RenewalManager : IRenewalManager
    {
        private static readonly RNGCryptoServiceProvider s_randomGenerator = new RNGCryptoServiceProvider(); // thread-safe

        public async Task Renew(RenewalParameters renewalParams)
        {
            if (renewalParams == null)
            {
                throw new ArgumentNullException(nameof(renewalParams));
            }

            Trace.TraceInformation("Generating SSL certificate with parameters: {0}", renewalParams);

            Trace.TraceInformation("Generating secure PFX password for '{0}'...", renewalParams.WebApp);
            byte[] pfxPassData = new byte[32];
            s_randomGenerator.GetBytes(pfxPassData);

            Trace.TraceInformation("Adding SSL cert for '{0}'...", renewalParams.WebApp);
            var azureWebAppEnvironment = new AzureWebAppEnvironment(
                renewalParams.TenantId,
                renewalParams.SubscriptionId,
                renewalParams.ClientId,
                renewalParams.ClientSecret,
                renewalParams.ResourceGroup,
                renewalParams.WebApp,
                renewalParams.ServicePlanResourceGroup,
                renewalParams.SiteSlotName);

            var manager = new CertificateManager(
                azureWebAppEnvironment,
                new AcmeConfig
                {
                    Host = renewalParams.Hosts[0],
                    AlternateNames = renewalParams.Hosts.Skip(1).ToList(),
                    RegistrationEmail = renewalParams.Email,
                    RSAKeyLength = renewalParams.RsaKeyLength,
                    PFXPassword = Convert.ToBase64String(pfxPassData),
#pragma warning disable S1075
                    BaseUri = (renewalParams.AcmeBaseUri ?? new Uri("https://acme-v01.api.letsencrypt.org/")).ToString()
#pragma warning restore S1075
                },
                new WebAppCertificateService(azureWebAppEnvironment, new CertificateServiceSettings { UseIPBasedSSL = renewalParams.UseIpBasedSsl }),
                new KuduFileSystemAuthorizationChallengeProvider(azureWebAppEnvironment, new AuthProviderConfig()));

            await manager.AddCertificate();

            Trace.TraceInformation("SSL cert added successfully to '{0}'", renewalParams.WebApp);
        }
    }
}