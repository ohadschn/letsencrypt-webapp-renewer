using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.Mocks;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.Util;
using AppSettingsReader = OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings.AppSettingsReader;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.WebJob
{
    [TestClass]
    public class AppSettingsTests : RenewalTestBase
    {
        public const string KeyPrefix = "letsencrypt:";

        private const string WebAppsKey = "webApps";
        private const string SubscriptionIdKeySuffix = "subscriptionId";
        private const string TenantIdKeySuffix = "tenantId";
        private const string ResourceGroupKeySuffix = "resourceGroup";
        private const string HostsKeySuffix = "hosts";
        private const string EmailKeySuffix = "email";
        private const string ClientIdKeySuffix = "clientId";
        private const string ClientSecretKeySuffix = "clientSecret";
        private const string ServicePlanResourceGroupKeySuffix = "servicePlanResourceGroup";
        private const string SiteSlotNameSuffix = "siteSlotName";
        private const string UseIpBasedSslKeySuffix = "useIpBasedSsl";
        private const string RsaKeyLengthKeySuffix = "rsaKeyLength";
        private const string AcmeBaseUriKeySuffix = "acmeBaseUri";
        private const string RenewXNumberOfDaysBeforeExpirationKeySuffix = "renewXNumberOfDaysBeforeExpiration";
        private const string AzureAuthenticationEndpointKeySuffix = "azureAuthenticationEndpoint";
        private const string AzureTokenAudienceKeySuffix = "azureTokenAudience";
        private const string AzureManagementEndpointKeySuffix = "azureManagementEndpoint";
        private const string AzureDefaultWebSiteDomainNameKeySuffix = "azureDefaultWebSiteDomainName";

        private readonly AppSettingsRenewer m_renewer;

        private readonly NameValueCollection m_appSettings = new NameValueCollection
        {
            // Web App collection declaration
            { BuildConfigKey(WebAppsKey), WebApp1 + ";" + WebApp2 },

            // Shared
            { BuildConfigKey(ClientIdKeySuffix), ClientId1.ToString() },
            { BuildConfigKey(RenewXNumberOfDaysBeforeExpirationKeySuffix), RenewXNumberOfDaysBeforeExpiration.ToString(CultureInfo.InvariantCulture) },

            // WebApp1
            { BuildConfigKey(SubscriptionIdKeySuffix, WebApp1), Subscription1.ToString() },
            { BuildConfigKey(ResourceGroupKeySuffix, WebApp1), ResourceGroup1 },
            { BuildConfigKey(TenantIdKeySuffix, WebApp1), Tenant1 },
            { BuildConfigKey(HostsKeySuffix, WebApp1), String.Join(";", Hosts1) },
            { BuildConfigKey(EmailKeySuffix, WebApp1), Email1 },
            { BuildConfigKey(SiteSlotNameSuffix, WebApp1), SiteSlotName1 },
            { BuildConfigKey(UseIpBasedSslKeySuffix, WebApp1), UseIpBasedSsl1.ToString() },
            { BuildConfigKey(RsaKeyLengthKeySuffix, WebApp1), RsaKeyLength1.ToString(CultureInfo.InvariantCulture) },
            { BuildConfigKey(AcmeBaseUriKeySuffix, WebApp1), AcmeBaseUri1.ToString() },
            { BuildConfigKey(ServicePlanResourceGroupKeySuffix, WebApp1), ServicePlanResourceGroup1 },
            { BuildConfigKey(AzureAuthenticationEndpointKeySuffix, WebApp1), AzureAuthenticationEndpoint1.ToString() },
            { BuildConfigKey(AzureTokenAudienceKeySuffix, WebApp1), AzureTokenAudience1.ToString() },
            { BuildConfigKey(AzureManagementEndpointKeySuffix, WebApp1), AzureManagementEndpoint1.ToString() },
            { BuildConfigKey(AzureDefaultWebSiteDomainNameKeySuffix, WebApp1), AzureDefaultWebsiteDomainName1 },

            // WebApp2
            { BuildConfigKey(SubscriptionIdKeySuffix, WebApp2), Subscription2.ToString() },
            { BuildConfigKey(TenantIdKeySuffix, WebApp2), Tenant2 },
            { BuildConfigKey(ResourceGroupKeySuffix, WebApp2), ResourceGroup2 },
            { BuildConfigKey(HostsKeySuffix, WebApp2), String.Join(";", Hosts2) },
            { BuildConfigKey(EmailKeySuffix, WebApp2), Email2 },
            { BuildConfigKey(ClientIdKeySuffix, WebApp2), ClientId2.ToString() }
        };

        private readonly ConnectionStringSettingsCollection m_connectionStrings = new ConnectionStringSettingsCollection
        {
            // Shared
            new ConnectionStringSettings(BuildConfigKey(ClientSecretKeySuffix), ClientSecret1),

            // WebApp2 override
            new ConnectionStringSettings(BuildConfigKey(ClientSecretKeySuffix, WebApp2), ClientSecret2)
        };

        private readonly EmailNotifierMock m_emailNotifier = new EmailNotifierMock();

        public AppSettingsTests()
        {
            m_renewer = new AppSettingsRenewer(
                RenewalManager,
                new AppSettingsRenewalParamsReader(new AppSettingsReader(m_appSettings, m_connectionStrings)),
                m_emailNotifier);
        }

        [TestMethod]
        public async Task TestSingleWebAppConfig()
        {
            m_appSettings[KeyPrefix + WebAppsKey] = WebApp1;
            await m_renewer.Renew();
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1);
            VerifySuccessfulRenewal(new[] { ExpectedFullRenewalParameters1 }, m_emailNotifier.RenewalParameters);
        }

        [TestMethod]
        public async Task TestDoubleWebAppConfig()
        {
            await m_renewer.Renew();
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1, ExpectedPartialRenewalParameters2);
            VerifySuccessfulRenewal(new[] { ExpectedFullRenewalParameters1, ExpectedPartialRenewalParameters2 }, m_emailNotifier.RenewalParameters);
        }

        [TestMethod]
        public void TestInvalidWebApps()
        {
            AssertInvalidConfig(BuildConfigKey(WebAppsKey), String.Empty, "webApp");
        }

        [TestMethod]
        public void TestMissingWebApp()
        {
            AssertInvalidConfig(BuildConfigKey(WebAppsKey), "hello");
        }

        [TestMethod]
        public void TestInvalidSubscriptionId()
        {
            AssertInvalidConfig(BuildConfigKey(SubscriptionIdKeySuffix, WebApp1), "not a GUID", "subscriptionId");
        }

        [TestMethod]
        public void TestInvalidTenantId()
        {
            AssertInvalidConfig(BuildConfigKey(TenantIdKeySuffix, WebApp2), String.Empty, "tenantId");
        }

        [TestMethod]
        public void TestInvalidResourceGroup()
        {
            AssertInvalidConfig(BuildConfigKey(ResourceGroupKeySuffix, WebApp1), "     ", "resourceGroup");
        }

        [TestMethod]
        public void TestInvalidHosts()
        {
            AssertInvalidConfig(BuildConfigKey(HostsKeySuffix, WebApp2), "www.foo.com;not/valid", "hosts");
        }

        [TestMethod]
        public void TestInvalidEmail()
        {
            AssertInvalidConfig(BuildConfigKey(EmailKeySuffix, WebApp1), "mail@", "email");
        }

        [TestMethod]
        public void TestInvalidClientId()
        {
            AssertInvalidConfig(BuildConfigKey(ClientIdKeySuffix, WebApp2), " ", "clientId");
        }

        [TestMethod]
        public void TestInvalidServicePlanResourceGroup()
        {
            AssertInvalidConfig(BuildConfigKey(ServicePlanResourceGroupKeySuffix, WebApp1), String.Empty, "servicePlanResourceGroup");
        }

        [TestMethod]
        public void TestInvalidSiteSlotName()
        {
            AssertInvalidConfig(BuildConfigKey(SiteSlotNameSuffix, WebApp2), " ", "siteSlotName");
        }

        [TestMethod]
        public void TestInvalidClientSecret()
        {
            var sharedClientSecretKey = BuildConfigKey(ClientSecretKeySuffix);
            var clientSecretKey = BuildConfigKey(ClientSecretKeySuffix, WebApp1);
            m_connectionStrings.Remove(clientSecretKey);
            m_connectionStrings.Remove(sharedClientSecretKey);
            AssertExtensions.Throws<ConfigurationErrorsException>(() => m_renewer.Renew().GetAwaiter().GetResult(), e => e.ToString().Contains("clientSecret"));

            m_connectionStrings.Add(new ConnectionStringSettings(clientSecretKey, " "));
            AssertExtensions.Throws<ConfigurationErrorsException>(() => m_renewer.Renew().GetAwaiter().GetResult(), e => e.ToString().Contains("clientSecret"));
        }

        [TestMethod]
        public void TestInvalidUseIpBasedSsl()
        {
            AssertInvalidConfig(BuildConfigKey(UseIpBasedSslKeySuffix, WebApp2), String.Empty, "useIpBasedSsl");
        }

        [TestMethod]
        public void TestInvalidRsaKeyLength()
        {
            AssertInvalidConfig(BuildConfigKey(RsaKeyLengthKeySuffix, WebApp1), "x", "rsaKeyLength");
        }

        [TestMethod]
        public void TestInvalidAcmeBaseUri()
        {
            AssertInvalidConfig(BuildConfigKey(AcmeBaseUriKeySuffix, WebApp2), "http:/OnlyOneSlash.com", "acmeBaseUri");
        }

        [TestMethod]
        public void TestInvalidSharedSetting()
        {
            AssertInvalidConfig(BuildConfigKey(UseIpBasedSslKeySuffix), "maybe false, maybe true - who knows?", "useIpBasedSsl");
        }

        private void AssertInvalidConfig(string key, string value, string expectedText = null)
        {
            m_appSettings[key] = value;
            AssertExtensions.Throws<ConfigurationErrorsException>(
                () => m_renewer.Renew().GetAwaiter().GetResult(),
                e => expectedText == null || e.Message.Contains(expectedText) || e.ToString().Contains(expectedText));
        }

        private static string BuildConfigKey(string key, string webApp = null)
        {
            var builder = new StringBuilder(KeyPrefix);
            if (webApp != null)
            {
                builder.Append(webApp + "-");
            }

            builder.Append(key);
            return builder.ToString();
        }
    }
}