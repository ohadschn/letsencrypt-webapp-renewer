using System;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public sealed class SharedRenewalParameters
    {
        public SharedRenewalParameters(
            AzureEnvironmentParams webAppEnvironment,
            string email,
            string fromEmail,
            string servicePlanResourceGroup,
            AzureEnvironmentParams azureDnsEnvironment,
            string azureDnsZoneName,
            string azureDnsRelativeRecordSetName,
            GoDaddyEnvironmentParams goDaddyDnsEnvironment,
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
            FromEmail = fromEmail;
            ServicePlanResourceGroup = servicePlanResourceGroup;
            AzureDnsEnvironment = azureDnsEnvironment;
            AzureDnsZoneName = azureDnsZoneName;
            AzureDnsRelativeRecordSetName = azureDnsRelativeRecordSetName;
            GoDaddyDnsEnvironment = goDaddyDnsEnvironment;
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
        public string FromEmail { get; }
        public string ServicePlanResourceGroup { get; }
        public AzureEnvironmentParams AzureDnsEnvironment { get; }
        public string AzureDnsZoneName { get; }
        public string AzureDnsRelativeRecordSetName { get; }
        public GoDaddyEnvironmentParams GoDaddyDnsEnvironment { get; }
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
            return Invariant($"{nameof(WebAppEnvironment)}: {WebAppEnvironment}, {nameof(Email)}: {Email}, {nameof(ServicePlanResourceGroup)}: {ServicePlanResourceGroup}, {nameof(AzureDnsEnvironment)}: {AzureDnsEnvironment}, {nameof(AzureDnsZoneName)}: {AzureDnsZoneName}, {nameof(AzureDnsRelativeRecordSetName)}: {AzureDnsRelativeRecordSetName}, {nameof(GoDaddyDnsEnvironment)}: {GoDaddyDnsEnvironment}, {nameof(UseIpBasedSsl)}: {UseIpBasedSsl}, {nameof(RsaKeyLength)}: {RsaKeyLength}, {nameof(AcmeBaseUri)}: {AcmeBaseUri}, {nameof(WebRootPath)}: {WebRootPath}, {nameof(RenewXNumberOfDaysBeforeExpiration)}: {RenewXNumberOfDaysBeforeExpiration}, {nameof(AuthenticationUri)}: {AuthenticationUri}, {nameof(AzureTokenAudience)}: {AzureTokenAudience}, {nameof(AzureManagementEndpoint)}: {AzureManagementEndpoint}, {nameof(AzureDefaultWebsiteDomainName)}: {AzureDefaultWebsiteDomainName}");
        }
    }
}