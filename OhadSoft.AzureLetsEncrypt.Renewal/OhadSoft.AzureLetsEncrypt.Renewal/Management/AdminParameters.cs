using System;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public sealed class AdminParameters
    {
        public AdminParameters(string resourceGroup, Guid subscriptionId, string tenantId, Guid clientId, string clientSecret, string email, string servicePlanResourceGroup)
        {
            ResourceGroup = resourceGroup;
            SubscriptionId = subscriptionId;
            TenantId = tenantId;
            ClientId = clientId;
            ClientSecret = clientSecret;
            Email = email;
            ServicePlanResourceGroup = servicePlanResourceGroup;
        }

        public string ResourceGroup { get; }
        public Guid SubscriptionId { get; }
        public string TenantId { get; }
        public Guid ClientId { get; }
        public string ClientSecret { get; }
        public string Email { get; }
        public string ServicePlanResourceGroup { get; }

        public override string ToString()
        {
            return Invariant($"{nameof(ResourceGroup)}: {ResourceGroup}, {nameof(SubscriptionId)}: {SubscriptionId}, {nameof(TenantId)}: {TenantId}, {nameof(ClientId)}: {ClientId}, {nameof(ClientSecret)}: {(String.IsNullOrWhiteSpace(ClientSecret) ? "<UNSPECIFIED>" : "<SCRUBBED>")}, {nameof(Email)}: {Email}, {nameof(ServicePlanResourceGroup)}: {ServicePlanResourceGroup}");
        }
    }
}