using System;
using System.Security.Cryptography;
using LetsEncrypt.Azure.Core;
using LetsEncrypt.Azure.Core.Models;

namespace OhadSoft.AzureLetsEncrypt.Renewal
{
    public class RenewalManager
    {
        private static readonly RNGCryptoServiceProvider s_randomGenerator = new RNGCryptoServiceProvider(); //thread-safe

        private readonly int _rsaKeyLength;
        private readonly bool _useIpBasedSsl;
        private readonly Uri _acmeBasedUri;

        public RenewalManager(bool useIpBasedSsl = false, int rsaKeyLength = 2048, Uri acmeBasedUri = null)
        {
            _rsaKeyLength = rsaKeyLength;
            _useIpBasedSsl = useIpBasedSsl;
            _acmeBasedUri = acmeBasedUri ?? new Uri("https://acme-v01.api.letsencrypt.org/");
        }

        public void Renew(string tenantId, Guid subscriptionId, Guid clientId, string clientSecret, string resourceGroup, string webApp, string host, string email)
        {
            byte[] pfxPassData = new byte[32];
            s_randomGenerator.GetBytes(pfxPassData);

            var manager = new CertificateManager(
                new AzureEnvironment(tenantId, subscriptionId, clientId, clientSecret, resourceGroup, webApp),
                new AcmeConfig
                {
                    Host = host,
                    RegistrationEmail = email,
                    RSAKeyLength = _rsaKeyLength,
                    PFXPassword = Convert.ToBase64String(pfxPassData),
                    BaseUri = _acmeBasedUri.ToString()
                }, 
                new CertificateServiceSettings { UseIPBasedSSL = _useIpBasedSsl }, 
                new AuthProviderConfig());

            manager.AddCertificate();
        }
    }
}