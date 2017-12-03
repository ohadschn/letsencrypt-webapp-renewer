using System;
using System.Collections.Generic;
using CommandLine;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Cli
{
    public class Options
    {
        public Options(
            Guid subscriptionId,
            string tenantId,
            string resourceGroup,
            string webApp,
            IReadOnlyList<string> hosts,
            string email,
            Guid clientId,
            string clientSecret,
            OptionalString servicePlanResourceGroup,
            OptionalString siteSlotName,
            bool useIpBasedSsl,
            int rsaKeyLength,
            Uri acmeBaseUri,
            int renewXNumberOfDaysBeforeExpiration,
            Uri azureAuthenticationEndpoint,
            Uri azureTokenAudience,
            Uri azureManagementEndpoint,
            OptionalString azureDefaultWebsiteDomainName)
        {
            SubscriptionId = subscriptionId;
            TenantId = tenantId;
            ResourceGroup = resourceGroup;
            WebApp = webApp;
            Hosts = hosts;
            Email = email;
            ClientId = clientId;
            ClientSecret = clientSecret;
            ServicePlanResourceGroup = servicePlanResourceGroup;
            SiteSlotName = siteSlotName;
            UseIpBasedSsl = useIpBasedSsl;
            RsaKeyLength = rsaKeyLength;
            AcmeBaseUri = acmeBaseUri;
            RenewXNumberOfDaysBeforeExpiration = renewXNumberOfDaysBeforeExpiration;
            AzureAuthenticationEndpoint = azureAuthenticationEndpoint;
            AzureTokenAudience = azureTokenAudience;
            AzureManagementEndpoint = azureManagementEndpoint;
            AzureDefaultWebsiteDomainName = azureDefaultWebsiteDomainName;
        }

        [Option('s', Constants.SubscriptionIdKey, Required = true, HelpText = "Subscription ID")]
        public Guid SubscriptionId { get; }

        [Option('t', Constants.TenantIdKey, Required = true, HelpText = "Tenant ID")]
        public string TenantId { get; }

        [Option('r', Constants.ResourceGroupKey, Required = true, HelpText = "Resource Group")]
        public string ResourceGroup { get; }

        [Option('w', "webApp", Required = true, HelpText = "Web App")]
        public string WebApp { get; }

        [Option('o', Constants.HostsKey, Required = true, Separator = ';', HelpText = "Semicolon-delimited list of hosts to include in the certificate - the first will comprise the Subject Name (SN) and the rest will comprise the Subject Alternative Names (SANs)")]
        public IReadOnlyList<string> Hosts { get; }

        [Option('e', Constants.EmailKey, Required = true, HelpText = "E-mail for Let's Encrypt registration and expiry notifications")]
        public string Email { get; }

        [Option('c', Constants.ClientIdKey, Required = true, HelpText = "Client ID")]
        public Guid ClientId { get; }

        [Option('l', Constants.ClientSecretKey, Required = true, HelpText = "Client Secret")]
        public string ClientSecret { get; }

        [Option('p', Constants.ServicePlanResourceGroupKey, Required = false, HelpText = "Service Plan Resource Group (if not specified, the provided Web App resource group will be used)")]
        public OptionalString ServicePlanResourceGroup { get; }

        [Option('d', Constants.SiteSlotNameKey, Required = false, HelpText = "Site Deployment Slot")]
        public OptionalString SiteSlotName { get; }

        [Option('i', Constants.UseIpBasedSslKey, Required = false, Default = false, HelpText = "Use IP Based SSL")]
        public bool UseIpBasedSsl { get; }

        [Option('k', Constants.RsaKeyLengthKey, Required = false, Default = 2048, HelpText = "Certificate RSA key length")]
        public int RsaKeyLength { get; }

        [Option('a', Constants.AcmeBaseUriKey, Required = false, HelpText = "ACME base URI, defaults to: " + RenewalManager.DefaultAcmeBaseUri)]
        public Uri AcmeBaseUri { get; }

        [Option('n', Constants.RenewXNumberOfDaysBeforeExpirationKey, Required = false, Default = -1, HelpText = "Number of days before certificate expiry to renew, defaults to a negative value meaning renewal will take place regardless of the expiry time")]
        public int RenewXNumberOfDaysBeforeExpiration { get; }

        [Option('h', Constants.AzureAuthenticationEndpointKey, Required = false, HelpText = "The Active Directory Authority, defaults to: " + RenewalManager.DefaultAuthenticationUri)]
        public Uri AzureAuthenticationEndpoint { get; }

        [Option('u', Constants.AzureTokenAudienceKey, Required = false, HelpText = "The Active Directory Service Endpoint Resource ID, defaults to: " + RenewalManager.DefaultAzureTokenAudienceService)]
        public Uri AzureTokenAudience { get; }

        [Option('m', Constants.AzureManagementEndpointKey, Required = false, HelpText = "The Resource Manager URL, defaults to: " + RenewalManager.DefaultManagementEndpoint)]
        public Uri AzureManagementEndpoint { get; }

        [Option('b', Constants.AzureDefaultWebSiteDomainNameKey, Required = false, HelpText = "The Azure Web Sites default domain name, defaults to: " + RenewalManager.DefaultWebsiteDomainName)]
        public OptionalString AzureDefaultWebsiteDomainName { get; }
    }
}