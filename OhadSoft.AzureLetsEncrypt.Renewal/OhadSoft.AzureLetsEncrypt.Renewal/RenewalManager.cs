using System;
using System.Linq;
using System.Security.Cryptography;
using LetsEncrypt.Azure.Core;
using LetsEncrypt.Azure.Core.Models;

namespace OhadSoft.AzureLetsEncrypt.Renewal
{
    public class RenewalManager : IRenewalManager
    {
        private static readonly RNGCryptoServiceProvider s_randomGenerator = new RNGCryptoServiceProvider(); //thread-safe

        public void Renew(RenewalParameters renewParams)
        {
            if (renewParams == null)
            {
                throw new ArgumentNullException(nameof(renewParams));
            }

            byte[] pfxPassData = new byte[32];
            s_randomGenerator.GetBytes(pfxPassData);

            var manager = new CertificateManager(
                new AzureEnvironment(
                    renewParams.TenantId, 
                    renewParams.SubscriptionId, 
                    renewParams.ClientId, 
                    renewParams.ClientSecret, 
                    renewParams.ResourceGroup, 
                    renewParams.WebApp),
                new AcmeConfig
                {
                    Host = renewParams.Hosts[0],
                    AlternateNames = renewParams.Hosts.Skip(1).ToList(),
                    RegistrationEmail = renewParams.Email,
                    RSAKeyLength = renewParams.RsaKeyLength,
                    PFXPassword = Convert.ToBase64String(pfxPassData),
                    BaseUri = (renewParams.AcmeBasedUri ?? new Uri("https://acme-v01.api.letsencrypt.org/")).ToString()
                }, 
                new CertificateServiceSettings { UseIPBasedSSL = renewParams.UseIpBasedSsl }, 
                new AuthProviderConfig());

            manager.AddCertificate();
        }
    }
}