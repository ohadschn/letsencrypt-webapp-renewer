using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using LetsEncrypt.Azure.Core;
using LetsEncrypt.Azure.Core.Models;
using OhadSoft.AzureLetsEncrypt.Renewal.Configuration;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public class RenewalManager : IRenewalManager
    {
        private static readonly RNGCryptoServiceProvider s_randomGenerator = new RNGCryptoServiceProvider(); // thread-safe

        public void Renew(RenewalParameters renewParams)
        {
            if (renewParams == null)
            {
                throw new ArgumentNullException(nameof(renewParams));
            }

            Trace.TraceInformation("Generating SSL certificate with parameters: {0}", renewParams);

            Trace.TraceInformation("Generating secure PFX password for '{0}'...", renewParams.WebApp);
            byte[] pfxPassData = new byte[32];
            s_randomGenerator.GetBytes(pfxPassData);

            Trace.TraceInformation("Adding SSL cert for '{0}'...", renewParams.WebApp);
            var manager = new CertificateManager(
                new AzureEnvironment(
                    renewParams.TenantId,
                    renewParams.SubscriptionId,
                    renewParams.ClientId,
                    renewParams.ClientSecret,
                    renewParams.ResourceGroup,
                    renewParams.WebApp,
                    renewParams.ServicePlanResourceGroup,
                    renewParams.SiteSlotName),
                new AcmeConfig
                {
                    Host = renewParams.Hosts[0],
                    AlternateNames = renewParams.Hosts.Skip(1).ToList(),
                    RegistrationEmail = renewParams.Email,
                    RSAKeyLength = renewParams.RsaKeyLength,
                    PFXPassword = Convert.ToBase64String(pfxPassData),
#pragma warning disable S1075
                    BaseUri = (renewParams.AcmeBaseUri ?? new Uri("https://acme-v01.api.letsencrypt.org/")).ToString()
#pragma warning restore S1075
                },
                new CertificateServiceSettings { UseIPBasedSSL = renewParams.UseIpBasedSsl },
                new AuthProviderConfig());

            manager.AddCertificate();

            Trace.TraceInformation("SSL cert added successfully to '{0}'", renewParams.WebApp);
        }
    }
}