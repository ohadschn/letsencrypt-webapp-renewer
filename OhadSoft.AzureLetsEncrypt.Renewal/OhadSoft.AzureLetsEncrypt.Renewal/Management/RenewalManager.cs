using System;
using System.Diagnostics;
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
#pragma warning disable S1075 // URIs should not be hardcoded
        public const string DefaultWebsiteDomainName = "azurewebsites.net";
        public const string DefaultAcmeBaseUri = "https://acme-v01.api.letsencrypt.org/";
        public const string DefaultAuthenticationUri = "https://login.windows.net/";
        public const string DefaultAzureTokenAudienceService = "https://management.core.windows.net/";
        public const string DefaultManagementEndpoint = "https://management.azure.com";
#pragma warning restore S1075 // URIs should not be hardcoded

        private static readonly RNGCryptoServiceProvider s_randomGenerator = new RNGCryptoServiceProvider(); // thread-safe

        public Task Renew(RenewalParameters renewalParams)
        {
            if (renewalParams == null)
            {
                throw new ArgumentNullException(nameof(renewalParams));
            }

            return RenewCore(renewalParams);
        }

        private static async Task RenewCore(RenewalParameters renewalParams)
        {
            Trace.TraceInformation("Generating SSL certificate with parameters: {0}", renewalParams);

            Trace.TraceInformation("Generating secure PFX password for '{0}'...", renewalParams.WebApp);
            var pfxPassData = new byte[32];
            s_randomGenerator.GetBytes(pfxPassData);

            Trace.TraceInformation(
                "Adding SSL cert for '{0}{1}'...",
                renewalParams.WebApp,
                renewalParams.GroupName == null ? String.Empty : $"[{renewalParams.GroupName}]");

            var azureEnvironment = new AzureWebAppEnvironment(
                    renewalParams.TenantId,
                    renewalParams.SubscriptionId,
                    renewalParams.ClientId,
                    renewalParams.ClientSecret,
                    renewalParams.ResourceGroup,
                    renewalParams.WebApp,
                    renewalParams.ServicePlanResourceGroup,
                    renewalParams.SiteSlotName)
            {
                WebRootPath = renewalParams.WebRootPath,
                AzureWebSitesDefaultDomainName = renewalParams.AzureDefaultWebsiteDomainName ?? DefaultWebsiteDomainName,
                AuthenticationEndpoint = renewalParams.AuthenticationUri ?? new Uri(DefaultAuthenticationUri),
                ManagementEndpoint = renewalParams.AzureManagementEndpoint ?? new Uri(DefaultManagementEndpoint),
                TokenAudience = renewalParams.AzureTokenAudience ?? new Uri(DefaultAzureTokenAudienceService)
            };

            var manager = CertificateManager.CreateKuduWebAppCertificateManager(
                azureEnvironment,
                new AcmeConfig
                {
                    Host = renewalParams.Hosts[0],
                    AlternateNames = renewalParams.Hosts.Skip(1).ToList(),
                    RegistrationEmail = renewalParams.Email,
                    RSAKeyLength = renewalParams.RsaKeyLength,
                    PFXPassword = Convert.ToBase64String(pfxPassData),
                    BaseUri = (renewalParams.AcmeBaseUri ?? new Uri(DefaultAcmeBaseUri)).ToString()
                },
                new CertificateServiceSettings { UseIPBasedSSL = renewalParams.UseIpBasedSsl },
                new AuthProviderConfig());

            if (await HasCertificate(azureEnvironment) && renewalParams.RenewXNumberOfDaysBeforeExpiration > 0)
            {
                await manager.RenewCertificate(false, renewalParams.RenewXNumberOfDaysBeforeExpiration);
            }
            else
            {
                await manager.AddCertificate();
            }

            Trace.TraceInformation("SSL cert added successfully to '{0}'", renewalParams.WebApp);
        }

        private static async Task<bool> HasCertificate(AzureWebAppEnvironment azureEnvironment)
        {
            using (var webSiteClient = await ArmHelper.GetWebSiteManagementClient(azureEnvironment))
            {
                var certs = await webSiteClient.Certificates.ListByResourceGroupWithHttpMessagesAsync(azureEnvironment.ServicePlanResourceGroupName);
                var site = webSiteClient.WebApps.GetSiteOrSlot(azureEnvironment.ResourceGroupName, azureEnvironment.WebAppName, azureEnvironment.SiteSlotName);

                return certs.Body.Any(s =>
                    s.Issuer.Contains("Let's Encrypt")
                    && site.HostNameSslStates.Any(hostNameBindings => hostNameBindings.Thumbprint == s.Thumbprint));
            }
        }
    }
}