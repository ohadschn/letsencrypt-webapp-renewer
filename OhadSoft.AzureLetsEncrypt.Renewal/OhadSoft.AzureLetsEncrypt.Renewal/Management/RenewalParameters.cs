using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public sealed class RenewalParameters : IEquatable<RenewalParameters>
    {
        public Guid SubscriptionId { get; }
        public string TenantId { get; }
        public string ResourceGroup { get; }
        public string WebApp { get; }
        public IReadOnlyList<string> Hosts { get; }
        public string Email { get; }
        public Guid ClientId { get; }
        public string ClientSecret { get; }
        public string ServicePlanResourceGroup { get; }
        public string SiteSlotName { get; }
        public bool UseIpBasedSsl { get; }
        public int RsaKeyLength { get; }
        public Uri AcmeBaseUri { get; }

        public RenewalParameters(
            Guid subscriptionId,
            string tenantId,
            string resourceGroup,
            string webApp,
            IReadOnlyList<string> hosts,
            string email,
            Guid clientId,
            string clientSecret,
            string servicePlanResourceGroup = null,
            string siteSlotName = null,
            bool useIpBasedSsl = false,
            int rsaKeyLength = 2048,
            Uri acmeBaseUri = null)
        {
            SubscriptionId = subscriptionId != Guid.Empty
                ? subscriptionId
                : throw new ArgumentException("Subscription ID must not be an empty GUID", nameof(subscriptionId));

            TenantId = !String.IsNullOrWhiteSpace(tenantId)
                ? tenantId
                : throw new ArgumentException("Tenant ID must not be null or whitespace", nameof(tenantId));

            ResourceGroup = !String.IsNullOrWhiteSpace(resourceGroup)
                ? resourceGroup
                : throw new ArgumentException("Resource group must not be null or whitespace", nameof(resourceGroup));

            WebApp = !String.IsNullOrWhiteSpace(webApp)
                ? webApp
                : throw new ArgumentException("Web app name must not be null or whitespace", nameof(webApp));

            Hosts = hosts != null && hosts.Count > 0 && hosts.All(h => Uri.CheckHostName(h) != UriHostNameType.Unknown)
                ? hosts
                : throw new ArgumentException("Host collection must be non-null, contain at least one element, and contain valid host names", nameof(hosts));

            Email = !String.IsNullOrWhiteSpace(email) && email.Contains("@") && email.Length >= 3 && email.Length <= 254
                    && !email.StartsWith("@", StringComparison.OrdinalIgnoreCase) && !email.EndsWith("@", StringComparison.OrdinalIgnoreCase)
                ? email
                : throw new ArgumentException("E-mail address must not be null and must be valid", nameof(email));

            ClientId = clientId != Guid.Empty ?
                clientId :
                throw new ArgumentException("Client ID must not be an empty GUID", nameof(clientId));

            ClientSecret = !String.IsNullOrWhiteSpace(clientSecret)
                ? clientSecret
                : throw new ArgumentException("Client secret must not be null or whitespace", nameof(clientSecret));

            ServicePlanResourceGroup = servicePlanResourceGroup == null || !servicePlanResourceGroup.All(Char.IsWhiteSpace)
                ? servicePlanResourceGroup
                : throw new ArgumentException("Service plan resource name must be either null or non-whitespace", nameof(servicePlanResourceGroup));

            SiteSlotName = siteSlotName == null || !siteSlotName.All(Char.IsWhiteSpace)
                ? siteSlotName
                : throw new ArgumentException("Site slot name must be either null or non-whitespace", nameof(siteSlotName));

            UseIpBasedSsl = useIpBasedSsl;

            RsaKeyLength = rsaKeyLength > 0
                ? rsaKeyLength
                : throw new ArgumentException("RSA key length must be positive", nameof(rsaKeyLength));

            AcmeBaseUri = acmeBaseUri == null || acmeBaseUri.IsAbsoluteUri
                ? acmeBaseUri
                : throw new ArgumentException("ACME base URI must be either null or absolute", nameof(acmeBaseUri));
        }

        public override string ToString()
        {
            return Invariant($"{nameof(SubscriptionId)}: {SubscriptionId}, {nameof(TenantId)}: {TenantId}, {nameof(ClientId)}: {ClientId}, {nameof(ClientSecret)}: [SCRUBBED], {nameof(ResourceGroup)}: {ResourceGroup}, {nameof(ServicePlanResourceGroup)}: {ServicePlanResourceGroup}, {nameof(WebApp)}: {WebApp}, {nameof(SiteSlotName)}: {SiteSlotName}, {nameof(Email)}: {Email}, {nameof(Hosts)}: {String.Join(", ", Hosts)}, {nameof(UseIpBasedSsl)}: {UseIpBasedSsl}, {nameof(RsaKeyLength)}: {RsaKeyLength}, {nameof(AcmeBaseUri)}: {AcmeBaseUri}");
        }

        public bool Equals(RenewalParameters other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return SubscriptionId.Equals(other.SubscriptionId)
                && string.Equals(TenantId, other.TenantId)
                && string.Equals(ResourceGroup, other.ResourceGroup)
                && string.Equals(WebApp, other.WebApp)
                && Hosts.SequenceEqual(other.Hosts)
                && string.Equals(Email, other.Email)
                && ClientId.Equals(other.ClientId)
                && string.Equals(ClientSecret, other.ClientSecret)
                && string.Equals(ServicePlanResourceGroup, other.ServicePlanResourceGroup)
                && string.Equals(SiteSlotName, other.SiteSlotName)
                && UseIpBasedSsl == other.UseIpBasedSsl
                && RsaKeyLength == other.RsaKeyLength
                && Equals(AcmeBaseUri, other.AcmeBaseUri);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var a = obj as RenewalParameters;
            return a != null && Equals(a);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SubscriptionId.GetHashCode();
                hashCode = (hashCode * 397) ^ (TenantId != null ? TenantId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ResourceGroup != null ? ResourceGroup.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WebApp != null ? WebApp.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Hosts != null ? Hosts.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Email != null ? Email.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ClientId.GetHashCode();
                hashCode = (hashCode * 397) ^ (ClientSecret != null ? ClientSecret.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ UseIpBasedSsl.GetHashCode();
                hashCode = (hashCode * 397) ^ RsaKeyLength;
                hashCode = (hashCode * 397) ^ (AcmeBaseUri != null ? AcmeBaseUri.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}