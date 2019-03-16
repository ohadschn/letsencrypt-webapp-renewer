using System;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public sealed class SharedRenewalParameters
    {
        public SharedRenewalParameters(
            string resourceGroup,
            Guid? subscriptionId,
            string tenantId,
            Guid? clientId,
            string clientSecret,
            string email,
            string servicePlanResourceGroup,
            bool? useIpBasedSsl,
            int? rsaKeyLength,
            Uri acmeBaseUri,
            string webRootPath,
            int? renewXNumberOfDaysBeforeExpiration,
            Uri authenticationUri,
            Uri azureTokenAudience,
            Uri azureManagementEndpoint,
            string azureDefaultWebsiteDomainName)
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
            WebRootPath = webRootPath;
            RenewXNumberOfDaysBeforeExpiration = renewXNumberOfDaysBeforeExpiration;
            AuthenticationUri = authenticationUri;
            AzureTokenAudience = azureTokenAudience;
            AzureManagementEndpoint = azureManagementEndpoint;
            AzureDefaultWebsiteDomainName = azureDefaultWebsiteDomainName;
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
        public string WebRootPath { get; }
        public int? RenewXNumberOfDaysBeforeExpiration { get; }
        public Uri AuthenticationUri { get; }
        public Uri AzureTokenAudience { get; }
        public Uri AzureManagementEndpoint { get; }
        public string AzureDefaultWebsiteDomainName { get; }

        public override string ToString()
        {
            return Invariant($"{nameof(ResourceGroup)}: {ResourceGroup}, {nameof(SubscriptionId)}: {SubscriptionId}, {nameof(TenantId)}: {TenantId}, {nameof(ClientId)}: {ClientId}, {nameof(ClientSecret)}: <SCRUBBED>, {nameof(Email)}: {Email}, {nameof(ServicePlanResourceGroup)}: {ServicePlanResourceGroup}, {nameof(UseIpBasedSsl)}: {UseIpBasedSsl}, {nameof(RsaKeyLength)}: {RsaKeyLength}, {nameof(AcmeBaseUri)}: {AcmeBaseUri}, {nameof(RenewXNumberOfDaysBeforeExpiration)}: {RenewXNumberOfDaysBeforeExpiration}, {nameof(AuthenticationUri)}: {AuthenticationUri}, {nameof(AzureTokenAudience)}: {AzureTokenAudience}, {nameof(AzureManagementEndpoint)}: {AzureManagementEndpoint}, {nameof(AzureDefaultWebsiteDomainName)}: {AzureDefaultWebsiteDomainName}");
        }
    }
}