using System;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public sealed class SharedRenewalParameters
    {
        public SharedRenewalParameters(
            AzureEnvironmentParams webAppEnvironment,
            string email,
            string servicePlanResourceGroup,
            AzureEnvironmentParams azureDnsEnvironment,
            string azureDnsZoneName,
            string azureDnsRelativeRecordSetName,
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
            WebAppEnvironment = webAppEnvironment;
            Email = email;
            ServicePlanResourceGroup = servicePlanResourceGroup;
            AzureDnsEnvironment = azureDnsEnvironment;
            AzureDnsZoneName = azureDnsZoneName;
            AzureDnsRelativeRecordSetName = azureDnsRelativeRecordSetName;
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

        public AzureEnvironmentParams WebAppEnvironment { get; }
        public string Email { get; }
        public string ServicePlanResourceGroup { get; }
        public AzureEnvironmentParams AzureDnsEnvironment { get; }
        public string AzureDnsZoneName { get; }
        public string AzureDnsRelativeRecordSetName { get; }
        public bool? UseIpBasedSsl { get; }
        public int? RsaKeyLength { get; }
        public Uri AcmeBaseUri { get; }
        public string WebRootPath { get; }
        public int? RenewXNumberOfDaysBeforeExpiration { get; }
        public Uri AuthenticationUri { get; }
        public Uri AzureTokenAudience { get; }
        public Uri AzureManagementEndpoint { get; }
        public string AzureDefaultWebsiteDomainName { get; }
    }
}