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
            Uri acmeBaseUri)
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
        }

        [Option('s', "subscriptionId", Required = true, HelpText = "Subscription ID")]
        public Guid SubscriptionId { get; }

        [Option('t', "tenantId", Required = true, HelpText = "Tenant ID")]
        public string TenantId { get; }

        [Option('r', "resourceGroup", Required = true, HelpText = "Resource Group")]
        public string ResourceGroup { get; }

        [Option('w', "webApp", Required = true, HelpText = "Web App")]
        public string WebApp { get; }

        [Option('o', "hosts", Required = true, Separator = ';', HelpText = "Semicolon-delimited list of hosts to include in the certificate - the first will comprise the Subject Name (SN) and the rest will comprise the Subject Alternative Names (SANs)")]
        public IReadOnlyList<string> Hosts { get; }

        [Option('e', "email", Required = true, HelpText = "E-mail for Let's Encrypt registration and expiry notifications")]
        public string Email { get; }

        [Option('c', "clientId", Required = true, HelpText = "Client ID")]
        public Guid ClientId { get; }

        [Option('l', "clientSecret", Required = true, HelpText = "Client Secret")]
        public string ClientSecret { get; }

        [Option('p', "servicePlanResourceGroup", Required = false, HelpText = "Service Plan Resource Group (if not specified, the provided Web App resource group will be used)")]
        public OptionalString ServicePlanResourceGroup { get; }

        [Option('d', "siteSlotName", Required = false, HelpText = "Site Deployment Slot")]
        public OptionalString SiteSlotName { get; }

        [Option('i', "useIpBasedSsl", Required = false, Default = false, HelpText = "Use IP Based SSL")]
        public bool UseIpBasedSsl { get; }

        [Option('k', "rsaKeyLength", Required = false, Default = 2048, HelpText = "Certificate RSA key length")]
        public int RsaKeyLength { get; }

        [Option('a', "acmeBaseUri", Required = false, HelpText = "ACME base URI, defaults to: " + RenewalManager.DefaultAcmeBaseUri)]
        public Uri AcmeBaseUri { get; }
    }
}