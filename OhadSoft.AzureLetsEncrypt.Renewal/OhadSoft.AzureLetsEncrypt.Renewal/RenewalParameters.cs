using System;
using System.Collections.Generic;

namespace OhadSoft.AzureLetsEncrypt.Renewal
{
    public sealed class RenewalParameters
    {
        public string TenantId { get; }
        public Guid SubscriptionId { get; }
        public Guid ClientId { get; }
        public string ClientSecret { get; }
        public string ResourceGroup { get; }
        public string WebApp { get; }
        public string Email { get; }
        public IReadOnlyList<string> Hosts { get; }
        public bool UseIpBasedSsl { get; }
        public int RsaKeyLength { get; }
        public Uri AcmeBasedUri { get; }

        public RenewalParameters(
            string tenantId, 
            Guid subscriptionId, 
            Guid clientId, 
            string clientSecret, 
            string resourceGroup, 
            string webApp, 
            string email, 
            IReadOnlyList<string> hosts, 
            bool useIpBasedSsl = false, 
            int rsaKeyLength = 2048, 
            Uri acmeBasedUri = null)
        {
            TenantId = !String.IsNullOrWhiteSpace(tenantId)
                ? tenantId
                : throw new ArgumentException("Tenant ID must not be null or whitespace", nameof(tenantId));

            SubscriptionId = subscriptionId != Guid.Empty
                ? subscriptionId
                : throw new ArgumentException("Subscription ID must not be an empty GUID", nameof(subscriptionId));

            ClientId = clientId != Guid.Empty ? 
                clientId : 
                throw new ArgumentException("Client ID must not be an empty GUID", nameof(clientId));

            ClientSecret = !String.IsNullOrWhiteSpace(clientSecret)
                ? clientSecret
                : throw new ArgumentException("Client secret must not be null or whitespace", nameof(clientSecret));

            ResourceGroup = !String.IsNullOrWhiteSpace(resourceGroup)
                ? resourceGroup
                : throw new ArgumentException("Resource group name must not be null or whitespace", nameof(resourceGroup));

            WebApp = !String.IsNullOrWhiteSpace(webApp)
                ? webApp
                : throw new ArgumentException("Web app name must not be null or whitespace", nameof(webApp));

            Email = !String.IsNullOrWhiteSpace(email)
                ? email
                : throw new ArgumentException("E-mail must not be null or whitespace", nameof(email));

            Hosts = hosts != null && hosts.Count > 0
                ? hosts
                : throw new ArgumentException("Host collection must be non-null and contain at least one element", nameof(hosts));

            AcmeBasedUri = acmeBasedUri == null || acmeBasedUri.IsAbsoluteUri
                ? acmeBasedUri
                : throw new ArgumentException("ACME base URI must be either null or absolute", nameof(acmeBasedUri));

            RsaKeyLength = rsaKeyLength > 0
                ? rsaKeyLength
                : throw new ArgumentException("RSA key length must be positive", nameof(rsaKeyLength));

            UseIpBasedSsl = useIpBasedSsl;
        }
    }
}