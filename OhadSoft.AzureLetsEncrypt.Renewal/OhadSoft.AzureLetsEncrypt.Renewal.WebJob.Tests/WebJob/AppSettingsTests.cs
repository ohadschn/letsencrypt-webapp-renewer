using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings;
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
            { BuildConfigKey(ClientIdKeySuffix), ClientIdShared.ToString() },

            // WebApp1
            { BuildConfigKey(SubscriptionIdKeySuffix, WebApp1), Subscription1.ToString() },
            { BuildConfigKey(ResourceGroupKeySuffix, WebApp1), ResourceGroup1 },
            { BuildConfigKey(TenantIdKeySuffix, WebApp1), Tenant1 },
            { BuildConfigKey(HostsKeySuffix, WebApp1), String.Join(";", Hosts1) },
            { BuildConfigKey(EmailKeySuffix, WebApp1), Email1 },
            { BuildConfigKey(ClientIdKeySuffix, WebApp1), ClientId1.ToString() }, // override shared
            { BuildConfigKey(UseIpBasedSslKeySuffix, WebApp1), UseIpBasedSsl1.ToString() },
            { BuildConfigKey(RsaKeyLengthKeySuffix, WebApp1), RsaKeyLength1.ToString(CultureInfo.InvariantCulture) },
            { BuildConfigKey(AcmeBaseUriKeySuffix, WebApp1), AcmeBaseUri1.ToString() },
            { BuildConfigKey(ServicePlanResourceGroupKeySuffix, WebApp1), ServicePlanResourceGroup1 },
            { BuildConfigKey(RenewXNumberOfDaysBeforeExpirationKeySuffix, WebApp1), RenewXNumberOfDaysBeforeExpiration1.ToString(CultureInfo.InvariantCulture) },
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
            { BuildConfigKey(ClientIdKeySuffix, WebApp2), ClientId2.ToString() } // override shared
        };

        private readonly ConnectionStringSettingsCollection m_connectionStrings = new ConnectionStringSettingsCollection
        {
            // Shared
            new ConnectionStringSettings(BuildConfigKey(ClientSecretKeySuffix), ClientSecret1),

            // override
            new ConnectionStringSettings(BuildConfigKey(ClientSecretKeySuffix, WebApp1), ClientSecret1),
            new ConnectionStringSettings(BuildConfigKey(ClientSecretKeySuffix, WebApp2), ClientSecret2)
        };

        public AppSettingsTests()
        {
            m_renewer = new AppSettingsRenewer(
                RenewalManager,
                new AppSettingsRenewalParamsReader(new AppSettingsReader(m_appSettings, m_connectionStrings)),
                EmailNotifier);
        }

        [TestMethod]
        public void TestInvalidWebApps()
        {
            AssertInvalidConfig(WebAppsKey, null, String.Empty, "webApp", testMissing: true, testShared: false);
        }

        [TestMethod]
        public void TestMissingWebAppConfiguration()
        {
            AssertInvalidConfig(WebAppsKey, null, "hello", expectedText: null, testMissing: true, testShared: false);
        }

        [TestMethod]
        public void TestInvalidSubscriptionId()
        {
            AssertInvalidConfig(SubscriptionIdKeySuffix, WebApp1, "not a GUID", "subscriptionId");
        }

        [TestMethod]
        public void TestInvalidTenantId()
        {
            AssertInvalidConfig(TenantIdKeySuffix, WebApp2, String.Empty, "tenantId");
        }

        [TestMethod]
        public void TestInvalidResourceGroup()
        {
            AssertInvalidConfig(ResourceGroupKeySuffix, WebApp1, "     ", "resourceGroup");
        }

        [TestMethod]
        public void TestInvalidHosts()
        {
            AssertInvalidConfig(HostsKeySuffix, WebApp2, "www.foo.com;not/valid", "hosts", testMissing: true, testShared: false);
        }

        [TestMethod]
        public void TestInvalidEmail()
        {
            AssertInvalidConfig(EmailKeySuffix, WebApp1, "mail@", "email");
        }

        [TestMethod]
        public void TestInvalidClientId()
        {
            AssertInvalidConfig(ClientIdKeySuffix, WebApp2, " ", "clientId");
        }

        [TestMethod]
        public void TestInvalidServicePlanResourceGroup()
        {
            AssertInvalidConfig(ServicePlanResourceGroupKeySuffix, WebApp1, String.Empty, "servicePlanResourceGroup", testMissing: false);
        }

        [TestMethod]
        public void TestInvalidSiteSlotName()
        {
            const string webApp = "foo{ }";

            m_appSettings[BuildConfigKey(WebAppsKey)] = webApp;
            m_appSettings[BuildConfigKey(SubscriptionIdKeySuffix, webApp)] = Subscription1.ToString();
            m_appSettings[BuildConfigKey(TenantIdKeySuffix, webApp)] = Tenant1;
            m_appSettings[BuildConfigKey(ResourceGroupKeySuffix, webApp)] = ResourceGroup1;
            m_appSettings[BuildConfigKey(HostsKeySuffix, webApp)] = String.Join(";", Hosts1);
            m_appSettings[BuildConfigKey(EmailKeySuffix, webApp)] = Email1;

            AssertInvalidConfigCore("siteSlotName");
        }

        [TestMethod]
        public void TestInvalidClientSecret()
        {
            var clientSecretKey = BuildConfigKey(ClientSecretKeySuffix, WebApp1);
            var sharedClientSecretKey = BuildConfigKey(ClientSecretKeySuffix);

            m_connectionStrings.Remove(clientSecretKey);
            m_connectionStrings.Remove(sharedClientSecretKey);
            AssertInvalidConfigCore("clientSecret");

            m_connectionStrings.Add(new ConnectionStringSettings(clientSecretKey, String.Empty));
            AssertInvalidConfigCore("clientSecret");

            m_connectionStrings.Remove(clientSecretKey);
            m_connectionStrings.Add(new ConnectionStringSettings(sharedClientSecretKey, String.Empty));
            AssertInvalidConfigCore("clientSecret");
        }

        [TestMethod]
        public void TestInvalidUseIpBasedSsl()
        {
            AssertInvalidConfig(UseIpBasedSslKeySuffix, WebApp2, String.Empty, "useIpBasedSsl", testMissing: false);
        }

        [TestMethod]
        public void TestInvalidRsaKeyLength()
        {
            AssertInvalidConfig(RsaKeyLengthKeySuffix, WebApp1, "x", "rsaKeyLength", testMissing: false);
        }

        [TestMethod]
        public void TestInvalidAcmeBaseUri()
        {
            AssertInvalidConfig(AcmeBaseUriKeySuffix, WebApp2, "http:/OnlyOneSlash.com", "acmeBaseUri", testMissing: false);
        }

        [TestMethod]
        public async Task TestSingleWebAppConfig()
        {
            m_appSettings[KeyPrefix + WebAppsKey] = WebApp1;
            await m_renewer.Renew();
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1);
            VerifySuccessfulNotification(ExpectedFullRenewalParameters1);
        }

        [TestMethod]
        public async Task TestDoubleWebAppConfig()
        {
            await m_renewer.Renew();
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1, ExpectedPartialRenewalParameters2);
            VerifySuccessfulNotification(ExpectedFullRenewalParameters1, ExpectedPartialRenewalParameters2);
        }

        [TestMethod]
        public async Task TestSharedSettings()
        {
            var appSettings = new NameValueCollection
            {
                { BuildConfigKey(WebAppsKey), WebApp3 },
                { BuildConfigKey(SubscriptionIdKeySuffix), SubscriptionShared.ToString() },
                { BuildConfigKey(ResourceGroupKeySuffix), ResourceGroupShared },
                { BuildConfigKey(TenantIdKeySuffix), TenantShared },
                { BuildConfigKey(HostsKeySuffix, WebApp3), String.Join(";", Hosts3) },
                { BuildConfigKey(EmailKeySuffix), EmailShared },
                { BuildConfigKey(ClientIdKeySuffix), ClientIdShared.ToString() },
                { BuildConfigKey(UseIpBasedSslKeySuffix), UseIpBasedSslShared.ToString() },
                { BuildConfigKey(RsaKeyLengthKeySuffix), RsaKeyLengthShared.ToString(CultureInfo.InvariantCulture) },
                { BuildConfigKey(AcmeBaseUriKeySuffix), AcmeBaseUriShared.ToString() },
                { BuildConfigKey(ServicePlanResourceGroupKeySuffix), ServicePlanResourceGroupShared },
                { BuildConfigKey(RenewXNumberOfDaysBeforeExpirationKeySuffix), RenewXNumberOfDaysBeforeExpirationShared.ToString(CultureInfo.InvariantCulture) },
                { BuildConfigKey(AzureAuthenticationEndpointKeySuffix), AzureAuthenticationEndpointShared.ToString() },
                { BuildConfigKey(AzureTokenAudienceKeySuffix), AzureTokenAudienceShared.ToString() },
                { BuildConfigKey(AzureManagementEndpointKeySuffix), AzureManagementEndpointShared.ToString() },
                { BuildConfigKey(AzureDefaultWebSiteDomainNameKeySuffix), AzureDefaultWebsiteDomainNameShared },
            };

            var connectionStrings = new ConnectionStringSettingsCollection
            {
                new ConnectionStringSettings(BuildConfigKey(ClientSecretKeySuffix), ClientSecretShared),
            };

            var renewer = new AppSettingsRenewer(
                RenewalManager,
                new AppSettingsRenewalParamsReader(new AppSettingsReader(appSettings, connectionStrings)),
                EmailNotifier);

            await renewer.Renew();

            VerifySuccessfulRenewal(ExpectedPartialRenewalParameters3);
            VerifySuccessfulNotification(ExpectedPartialRenewalParameters3);
        }

        [TestMethod]
        public async Task TestMultipleHostWebAppConfig()
        {
            m_appSettings[BuildConfigKey(WebAppsKey)] = WebApp4;
            m_appSettings[BuildConfigKey(SubscriptionIdKeySuffix, WebApp4)] = Subscription1.ToString();
            m_appSettings[BuildConfigKey(TenantIdKeySuffix, WebApp4)] = Tenant1;
            m_appSettings[BuildConfigKey(ClientIdKeySuffix, WebApp4)] = ClientId1.ToString();
            m_appSettings[BuildConfigKey(ClientSecretKeySuffix, WebApp4)] = ClientSecret1;
            m_appSettings[BuildConfigKey(ResourceGroupKeySuffix, WebApp4)] = ResourceGroup1;
            m_appSettings[BuildConfigKey(HostsKeySuffix, WebApp4)] = String.Join(";", Hosts4);
            m_appSettings[BuildConfigKey(EmailKeySuffix, WebApp4)] = Email1;

            await m_renewer.Renew();

            VerifySuccessfulRenewal(ExpectedPartialRenewalParameters4);
            VerifySuccessfulNotification(ExpectedPartialRenewalParameters4);
        }

        private void AssertInvalidConfig(string key, string webApp, string value, string expectedText = null, bool testMissing = true, bool testShared = true)
        {
            var configKey = BuildConfigKey(key, webApp);

            m_appSettings[configKey] = value;
            AssertInvalidConfigCore(expectedText);

            m_appSettings.Remove(configKey);
            var sharedCofigKey = BuildConfigKey(key);
            if (testShared)
            {
                m_appSettings[sharedCofigKey] = value;
                AssertInvalidConfigCore(expectedText);
            }

            if (testMissing)
            {
                m_appSettings.Remove(sharedCofigKey);
                AssertInvalidConfigCore(expectedText);
            }
        }

        private void AssertInvalidConfigCore(string expectedText)
        {
            AssertExtensions.Throws<ConfigurationErrorsException>(
                () => m_renewer.Renew().GetAwaiter().GetResult(),
                e => expectedText == null || e.ToString().Contains(expectedText));
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