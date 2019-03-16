namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob
{
    internal static class Constants
    {
        public const string KeyPrefix = "letsencrypt:";

        public const string SubscriptionIdKey = "subscriptionId";
        public const string TenantIdKey = "tenantId";
        public const string ResourceGroupKey = "resourceGroup";
        public const string HostsKey = "hosts";
        public const string EmailKey = "email";
        public const string ClientIdKey = "clientId";
        public const string ClientSecretKey = "clientSecret";
        public const string ServicePlanResourceGroupKey = "servicePlanResourceGroup";
        public const string SiteSlotNameKey = "siteSlotName";
        public const string UseIpBasedSslKey = "useIpBasedSsl";
        public const string RsaKeyLengthKey = "rsaKeyLength";
        public const string AcmeBaseUriKey = "acmeBaseUri";
        public const string WebRootPathKey = "webRootPath";
        public const string RenewXNumberOfDaysBeforeExpirationKey = "renewXNumberOfDaysBeforeExpiration";
        public const string AzureAuthenticationEndpointKey = "azureAuthenticationEndpoint";
        public const string AzureTokenAudienceKey = "azureTokenAudience";
        public const string AzureManagementEndpointKey = "azureManagementEndpoint";
        public const string AzureDefaultWebSiteDomainNameKey = "azureDefaultWebSiteDomainName";
        public const string SendGridApiKey = "SendGridApiKey";
    }
}