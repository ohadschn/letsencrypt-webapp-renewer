using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.Mocks;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.WebJob;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests
{
    public class RenewalTestBase
    {
        protected const string Tenant1 = "foo-tenant";
        protected const string Tenant2 = "bar-tenant";
        protected const string TenantShared = "shared-tenant";
        protected const string ResourceGroup1 = "foo-resource-group";
        protected const string ResourceGroup2 = "bar-resource-group";
        protected const string ResourceGroupShared = "shared-resource-group";
        protected const string SiteSlotName1 = "foo-slot";
        protected const string WebApp1Name = "fooApp";
        protected const string WebApp1 = WebApp1Name + "{" + SiteSlotName1 + "}";
        protected const string WebApp2 = "barApp";
        protected const string WebApp3 = "bazApp";
        protected const string WebApp4 = WebApp1Name + "[barBaz]";
        protected const string Email1 = "foo@gmail.com";
        protected const string Email2 = "bar@outlook.com";
        protected const string EmailShared = "shared@yahoo.com";
        protected const string ClientSecret1 = "foo-secret123";
        protected const string ClientSecret2 = "bar-verySecret321";
        protected const string ClientSecretShared = "shared-secret4real";
        protected const string ServicePlanResourceGroup1 = "foo-service-plan-resource-group";
        protected const string ServicePlanResourceGroupShared = "shared-service-plan-resource-group";
        protected const bool UseIpBasedSsl1 = true;
        protected const bool UseIpBasedSslShared = true;
        protected const int RsaKeyLength1 = 1024;
        protected const int RsaKeyLengthShared = 512;
        protected static readonly Guid Subscription1 = Guid.Parse("964bbd07-a237-489d-b060-53b027f56d35");
        protected static readonly Guid Subscription2 = Guid.Parse("c6068e1b-27a8-473d-9113-d443d1ae8859");
        protected static readonly Guid SubscriptionShared = Guid.Parse("53c40909-2b5c-49e9-8f2d-f32957ecfcf0");
        protected static readonly IReadOnlyList<string> Hosts1 = new[] { "www.foo.com" };
        protected static readonly IReadOnlyList<string> Hosts2 = new[] { "www.bar.com", "bar.com" };
        protected static readonly IReadOnlyList<string> Hosts3 = new[] { "www.shared.com" };
        protected static readonly IReadOnlyList<string> Hosts4 = new[] { "www.barbaz.com", "barbaz.com" };
        protected static readonly Guid ClientId1 = Guid.Parse("89e5c1ee-a37b-4af7-89fc-217a4b99652c");
        protected static readonly Guid ClientId2 = Guid.Parse("618a929b-d9c9-4ec1-b8dc-66f55d949d52");
        protected static readonly Guid ClientIdShared = Guid.Parse("a06ff82d-7964-45a0-920f-22897ab9f9cb");
        protected static readonly Uri AcmeBaseUri1 = new Uri("http://foo.example.com");
        protected static readonly Uri AcmeBaseUriShared = new Uri("http://shared.example.com");
        protected static readonly int RenewXNumberOfDaysBeforeExpiration1 = 22;
        protected static readonly int RenewXNumberOfDaysBeforeExpirationShared = 60;
        protected static readonly Uri AzureAuthenticationEndpoint1 = new Uri("https://authenticate.com");
        protected static readonly Uri AzureAuthenticationEndpointShared = new Uri("https://shared.authentication.com");
        protected static readonly Uri AzureTokenAudience1 = new Uri("https://www.tokens.com");
        protected static readonly Uri AzureTokenAudienceShared = new Uri("https://shared.tokens.com");
        protected static readonly Uri AzureManagementEndpoint1 = new Uri("https://manage.azure.ms");
        protected static readonly Uri AzureManagementEndpointShared = new Uri("https://shared.manage.azure.com");
        protected static readonly string AzureDefaultWebsiteDomainName1 = "websites.io";
        protected static readonly string AzureDefaultWebsiteDomainNameShared = "shared.websites.com";

        protected static readonly RenewalParameters ExpectedFullRenewalParameters1 = new RenewalParameters(
            Subscription1,
            Tenant1,
            ResourceGroup1,
            WebApp1Name,
            Hosts1,
            Email1,
            ClientId1,
            ClientSecret1,
            ServicePlanResourceGroup1,
            null, // groupName
            SiteSlotName1,
            UseIpBasedSsl1,
            RsaKeyLength1,
            AcmeBaseUri1,
            RenewXNumberOfDaysBeforeExpiration1,
            AzureAuthenticationEndpoint1,
            AzureTokenAudience1,
            AzureManagementEndpoint1,
            AzureDefaultWebsiteDomainName1);

        protected static readonly RenewalParameters ExpectedPartialRenewalParameters1 = new RenewalParameters(
            Subscription1,
            Tenant1,
            ResourceGroup1,
            WebApp1Name,
            Hosts1,
            Email1,
            ClientId1,
            ClientSecret1);

        protected static readonly RenewalParameters ExpectedPartialRenewalParameters2 = new RenewalParameters(
            Subscription2,
            Tenant2,
            ResourceGroup2,
            WebApp2,
            Hosts2,
            Email2,
            ClientId2,
            ClientSecret2);

        protected static readonly RenewalParameters ExpectedPartialRenewalParameters3 = new RenewalParameters(
            SubscriptionShared,
            TenantShared,
            ResourceGroupShared,
            WebApp3,
            Hosts3,
            EmailShared,
            ClientIdShared,
            ClientSecretShared,
            ServicePlanResourceGroupShared,
            null, // groupName
            null, // siteSlotName
            UseIpBasedSslShared,
            RsaKeyLengthShared,
            AcmeBaseUriShared,
            RenewXNumberOfDaysBeforeExpirationShared,
            AzureAuthenticationEndpointShared,
            AzureTokenAudienceShared,
            AzureManagementEndpointShared,
            AzureDefaultWebsiteDomainNameShared);

        protected static readonly RenewalParameters ExpectedPartialRenewalParameters4 = new RenewalParameters(
            Subscription1,
            Tenant1,
            ResourceGroup1,
            WebApp1Name,
            Hosts4,
            Email1,
            ClientId1,
            ClientSecret1);

        protected RenewalManagerMock RenewalManager { get; } = new RenewalManagerMock();
        protected EmailNotifierMock EmailNotifier { get; } = new EmailNotifierMock();

        protected void VerifySuccessfulRenewal(params RenewalParameters[] renewalParameters)
        {
            VerifySuccessfulRenewal(renewalParameters, RenewalManager.RenewalParameters);
        }

        protected void VerifySuccessfulNotification(params RenewalParameters[] renewalParameters)
        {
            VerifySuccessfulRenewal(renewalParameters, EmailNotifier.RenewalParameters);
        }

#pragma warning disable S3242
        private static void VerifySuccessfulRenewal(IReadOnlyCollection<RenewalParameters> expectedRenewalParams, IReadOnlyCollection<RenewalParameters> actualRenewalParams)
#pragma warning restore S3242
        {
            var nl = Environment.NewLine;
            var renewalParamsComparer = new RenewalParametersComparer();
            var actualSorted = actualRenewalParams.OrderBy(rp => rp, renewalParamsComparer).ToArray();
            var expectedSorted = expectedRenewalParams.OrderBy(rp => rp, renewalParamsComparer).ToArray();
            Assert.IsTrue(
                actualSorted.SequenceEqual(expectedSorted),
                Invariant($"Renewal parameter mismatch.{nl}Expected:{nl}{String.Join<RenewalParameters>(nl, expectedSorted)}{nl}Actual:{nl}{String.Join<RenewalParameters>(nl, actualSorted)}"));
        }
    }
}