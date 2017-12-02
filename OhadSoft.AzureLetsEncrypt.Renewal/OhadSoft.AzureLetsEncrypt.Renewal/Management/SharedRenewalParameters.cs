using System;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public sealed class SharedRenewalParameters
    {
        public SharedRenewalParameters(string resourceGroup, Guid? subscriptionId, string tenantId, Guid? clientId, string clientSecret, string email, string servicePlanResourceGroup, bool? useIpBasedSsl, int? rsaKeyLength, Uri acmeBaseUri, int? renewXNumberOfDaysBeforeExpiration)
        {
            ResourceGroup = resourceGroup;
            SubscriptionId = subscriptionId;
            TenantId = tenantId;
            ClientId = clientId;
            ClientSecret = clientSecret;
            Email = email;
            ServicePlanResourceGroup = servicePlanResourceGroup;
            UseIpBasedSsl = useIpBasedSsl;
            RsaKeyLength = rsaKeyLength;
            AcmeBaseUri = acmeBaseUri;
            RenewXNumberOfDaysBeforeExpiration = renewXNumberOfDaysBeforeExpiration;
        }

        public string ResourceGroup { get; }
        public Guid? SubscriptionId { get; }
        public string TenantId { get; }
        public Guid? ClientId { get; }
        public string ClientSecret { get; }
        public string Email { get; }
        public string ServicePlanResourceGroup { get; }
        public bool? UseIpBasedSsl { get; }
        public int? RsaKeyLength { get; }
        public Uri AcmeBaseUri { get; }
        public int? RenewXNumberOfDaysBeforeExpiration { get; }

        public override string ToString()
        {
            return Invariant($"{nameof(ResourceGroup)}: {ResourceGroup}, {nameof(SubscriptionId)}: {SubscriptionId}, {nameof(TenantId)}: {TenantId}, {nameof(ClientId)}: {ClientId}, {nameof(ClientSecret)}: {(String.IsNullOrWhiteSpace(ClientSecret) ? "<UNSPECIFIED>" : "<SCRUBBED>")}, {nameof(Email)}: {Email}, {nameof(ServicePlanResourceGroup)}: {ServicePlanResourceGroup}, {nameof(UseIpBasedSsl)}: {UseIpBasedSsl}, {nameof(RsaKeyLength)}: {RsaKeyLength}, {nameof(AcmeBaseUri)}: {AcmeBaseUri}");
        }
    }
}