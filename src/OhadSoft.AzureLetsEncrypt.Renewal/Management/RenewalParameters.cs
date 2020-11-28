using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OhadSoft.AzureLetsEncrypt.Renewal.Management.Util;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public sealed class RenewalParameters : IEquatable<RenewalParameters>
    {
        public string WebApp { get; }
        public IReadOnlyList<string> Hosts { get; }
        public string ToEmail { get; }
        public string FromEmail { get; }
        public string ServicePlanResourceGroup { get; }
        public string SiteSlotName { get; }
        public string GroupName { get; }
        public AzureEnvironmentParams WebAppEnvironmentParams { get; }
        public AzureEnvironmentParams AzureDnsEnvironmentParams { get; }
        public string AzureDnsZoneName { get; }
        public string AzureDnsRelativeRecordSetName { get; }
        public bool UseIpBasedSsl { get; }
        public int RsaKeyLength { get; }
        public Uri AcmeBaseUri { get; }
        public string WebRootPath { get; }
        public int RenewXNumberOfDaysBeforeExpiration { get; }
        public Uri AuthenticationUri { get; }
        public Uri AzureTokenAudience { get; }
        public Uri AzureManagementEndpoint { get; }
        public string AzureDefaultWebsiteDomainName { get; }

        public RenewalParameters(
            AzureEnvironmentParams webAppEnvironmentParams,
            string webApp,
            IReadOnlyList<string> hosts,
            string toEmail,
            string fromEmail,
            string servicePlanResourceGroup = null,
            string groupName = null,
            string siteSlotName = null,
            AzureEnvironmentParams azureDnsEnvironmentParams = null,
            string azureDnsZoneName = null,
            string azureDnsRelativeRecordSetName = null,
            bool useIpBasedSsl = false,
            int rsaKeyLength = 2048,
            Uri acmeBaseUri = null,
            string webRootPath = null,
            int renewXNumberOfDaysBeforeExpiration = -1,
            Uri authenticationUri = null,
            Uri azureTokenAudience = null,
            Uri azureManagementEndpoint = null,
            string azureDefaultWebsiteDomainName = null)
        {
            bool dnsChallenge = azureDnsZoneName != null && azureDnsRelativeRecordSetName != null;

            WebAppEnvironmentParams = ParamValidator.VerifyNonNull(webAppEnvironmentParams, nameof(webAppEnvironmentParams));
            WebApp = ParamValidator.VerifyString(webApp, nameof(webApp));
            Hosts = VerifyHosts(hosts, dnsChallenge, nameof(hosts));
            ToEmail = VerifyEmail(toEmail, nameof(toEmail));
            FromEmail = VerifyEmail(fromEmail, nameof(fromEmail));
            ServicePlanResourceGroup = ParamValidator.VerifyOptionalString(servicePlanResourceGroup, nameof(servicePlanResourceGroup));
            GroupName = ParamValidator.VerifyOptionalString(groupName, nameof(groupName));
            SiteSlotName = ParamValidator.VerifyOptionalString(siteSlotName, nameof(siteSlotName));
            AzureDnsEnvironmentParams = azureDnsEnvironmentParams ?? WebAppEnvironmentParams;
            AzureDnsZoneName = ParamValidator.VerifyOptionalString(azureDnsZoneName, nameof(azureDnsZoneName));
            AzureDnsRelativeRecordSetName = ParamValidator.VerifyOptionalString(azureDnsRelativeRecordSetName, nameof(azureDnsRelativeRecordSetName));
            UseIpBasedSsl = useIpBasedSsl;
            RsaKeyLength = ParamValidator.VerifyPositiveInteger(rsaKeyLength, nameof(rsaKeyLength));
            AcmeBaseUri = ParamValidator.VerifyOptionalUri(acmeBaseUri, nameof(acmeBaseUri));
            WebRootPath = ParamValidator.VerifyOptionalString(webRootPath, nameof(webRootPath));
            RenewXNumberOfDaysBeforeExpiration =
                VerifyRenewXNumberOfDaysBeforeExpiration(renewXNumberOfDaysBeforeExpiration, !dnsChallenge, nameof(renewXNumberOfDaysBeforeExpiration));
            AuthenticationUri = ParamValidator.VerifyOptionalUri(authenticationUri, nameof(authenticationUri));
            AzureTokenAudience = ParamValidator.VerifyOptionalUri(azureTokenAudience, nameof(azureTokenAudience));
            AzureManagementEndpoint = ParamValidator.VerifyOptionalUri(azureManagementEndpoint, nameof(azureManagementEndpoint));
            AzureDefaultWebsiteDomainName = ParamValidator.VerifyOptionalHostName(azureDefaultWebsiteDomainName, nameof(azureDefaultWebsiteDomainName));
        }

        public static IReadOnlyList<string> VerifyHosts(IReadOnlyList<string> hosts, bool enforceWildcard, string name)
        {
            if (hosts == null || hosts.Count == 0 || hosts.Any(h => Uri.CheckHostName(h?.Replace('*', 'x')) == UriHostNameType.Unknown))
            {
                throw new ArgumentException("Host collection must be non-null, contain at least one element, and contain valid host names", name);
            }

            if (enforceWildcard && hosts.Any(h => !h.StartsWith("*.", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Only wildcard host names are supported for the DNS challenge (must begin with '*.'", name);
            }

            return hosts;
        }

        public static string VerifyEmail(string email, string name)
        {
            return EmailHelper.IsValidEmail(email)
                ? email
                : throw new ArgumentException("E-mail address must not be null and must be valid", name);
        }

        public static int VerifyRenewXNumberOfDaysBeforeExpiration(int renewXNumberOfDaysBeforeExpiration, bool supported, string name)
        {
            return supported || renewXNumberOfDaysBeforeExpiration <= 0
                ? renewXNumberOfDaysBeforeExpiration
                : throw new ArgumentException("Expiration aware renewal not supported by DNS challenge", name);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(
                this,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented,
                });
        }

        public bool Equals(RenewalParameters other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(WebApp, other.WebApp, StringComparison.OrdinalIgnoreCase)
                   && Hosts.SequenceEqual(other.Hosts)
                   && string.Equals(ToEmail, other.ToEmail, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(FromEmail, other.FromEmail, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(ServicePlanResourceGroup, other.ServicePlanResourceGroup, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(SiteSlotName, other.SiteSlotName, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(GroupName, other.GroupName, StringComparison.OrdinalIgnoreCase)
                   && Equals(WebAppEnvironmentParams, other.WebAppEnvironmentParams)
                   && Equals(AzureDnsEnvironmentParams, other.AzureDnsEnvironmentParams)
                   && string.Equals(AzureDnsZoneName, other.AzureDnsZoneName, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(AzureDnsRelativeRecordSetName, other.AzureDnsRelativeRecordSetName, StringComparison.OrdinalIgnoreCase)
                   && UseIpBasedSsl == other.UseIpBasedSsl
                   && RsaKeyLength == other.RsaKeyLength
                   && Equals(AcmeBaseUri, other.AcmeBaseUri)
                   && string.Equals(WebRootPath, other.WebRootPath, StringComparison.OrdinalIgnoreCase)
                   && RenewXNumberOfDaysBeforeExpiration == other.RenewXNumberOfDaysBeforeExpiration
                   && Equals(AuthenticationUri, other.AuthenticationUri)
                   && Equals(AzureTokenAudience, other.AzureTokenAudience)
                   && Equals(AzureManagementEndpoint, other.AzureManagementEndpoint)
                   && string.Equals(AzureDefaultWebsiteDomainName, other.AzureDefaultWebsiteDomainName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is RenewalParameters other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = WebApp != null ? WebApp.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Hosts != null ? Hosts.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ToEmail != null ? ToEmail.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FromEmail != null ? FromEmail.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ServicePlanResourceGroup != null ? ServicePlanResourceGroup.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SiteSlotName != null ? SiteSlotName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (GroupName != null ? GroupName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WebAppEnvironmentParams != null ? WebAppEnvironmentParams.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureDnsEnvironmentParams != null ? AzureDnsEnvironmentParams.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureDnsZoneName != null ? AzureDnsZoneName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureDnsRelativeRecordSetName != null ? AzureDnsRelativeRecordSetName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ UseIpBasedSsl.GetHashCode();
                hashCode = (hashCode * 397) ^ RsaKeyLength;
                hashCode = (hashCode * 397) ^ (AcmeBaseUri != null ? AcmeBaseUri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WebRootPath != null ? WebRootPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RenewXNumberOfDaysBeforeExpiration;
                hashCode = (hashCode * 397) ^ (AuthenticationUri != null ? AuthenticationUri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureTokenAudience != null ? AzureTokenAudience.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureManagementEndpoint != null ? AzureManagementEndpoint.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AzureDefaultWebsiteDomainName != null ? AzureDefaultWebsiteDomainName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}