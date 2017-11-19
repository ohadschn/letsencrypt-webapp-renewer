using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Email;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.Util;
using AppSettingsReader = OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings.AppSettingsReader;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.WebJob
{
    [TestClass]
    public class AppSettingsTests : RenewalTestBase
    {
        public const string KeyPrefix = "letsencrypt:";

        public const string WebAppsKey = "webApps";
        public const string SubscriptionIdKeySuffix = "subscriptionId";
        public const string TenantIdKeySuffix = "tenantId";
        public const string ResourceGroupKeySuffix = "resourceGroup";
        public const string HostsKeySuffix = "hosts";
        public const string EmailKeySuffix = "email";
        public const string ClientIdKeySuffix = "clientId";
        public const string ClientSecretKeySuffix = "clientSecret";
        public const string ServicePlanResourceGroupKeySuffix = "servicePlanResourceGroup";
        public const string SiteSlotNameSuffix = "siteSlotName";
        public const string UseIpBasedSslKeySuffix = "useIpBasedSsl";
        public const string RsaKeyLengthKeySuffix = "rsaKeyLength";
        public const string AcmeBaseUriKeySuffix = "acmeBaseUri";

        private readonly AppSettingsRenewer m_renewer;

        private readonly NameValueCollection m_appSettings = new NameValueCollection
        {
            // Web App collection declaration
            { BuildConfigKey(WebAppsKey), WebApp1 + ";" + WebApp2 },

            // Shared
            { BuildConfigKey(ClientIdKeySuffix), ClientId1.ToString() },

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

        private readonly Mock<IEmailNotifier> m_emailNotifier = new Mock<IEmailNotifier>();
        private readonly ConcurrentQueue<RenewalParameters> m_notificationRenewalParameterses = new ConcurrentQueue<RenewalParameters>();

        public AppSettingsTests()
        {
            m_renewer = new AppSettingsRenewer(
                RenewalManager,
                new AppSettingsRenewalParamsReader(new AppSettingsReader(m_appSettings, m_connectionStrings)),
                m_emailNotifier.Object);

            m_emailNotifier
                .Setup(n => n.NotifyAsync(It.IsAny<RenewalParameters>()))
                .Returns(() => Task.CompletedTask)
                .Callback<RenewalParameters>(m_notificationRenewalParameterses.Enqueue);
        }

        [TestMethod]
        public void TestSingleWebAppConfig()
        {
            m_appSettings[KeyPrefix + WebAppsKey] = WebApp1;
            m_renewer.Renew();
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1);
            VerifySuccessfulRenewal(new[] { ExpectedFullRenewalParameters1 }, m_notificationRenewalParameterses);
        }

        [TestMethod]
        public void TestDoubleWebAppConfig()
        {
            m_renewer.Renew();
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1, ExpectedPartialRenewalParameters2);
            VerifySuccessfulRenewal(new[] { ExpectedFullRenewalParameters1, ExpectedPartialRenewalParameters2 }, m_notificationRenewalParameterses);
        }

        [TestMethod]
        public void TestInvalidWebApps()
        {
            AssertInvalidConfig(BuildConfigKey(WebAppsKey), String.Empty);
        }

        [TestMethod]
        public void TestMissingWebApp()
        {
            AssertInvalidConfig(BuildConfigKey(WebAppsKey), "hello");
        }

        [TestMethod]
        public void TestInvalidSubscriptionId()
        {
            AssertInvalidConfig(BuildConfigKey(SubscriptionIdKeySuffix, WebApp1), "not a GUID");
        }

        [TestMethod]
        public void TestInvalidTenantId()
        {
            AssertInvalidConfig(BuildConfigKey(TenantIdKeySuffix, WebApp2), String.Empty);
        }

        [TestMethod]
        public void TestInvalidResourceGroup()
        {
            AssertInvalidConfig(BuildConfigKey(ResourceGroupKeySuffix, WebApp1), "     ");
        }

        [TestMethod]
        public void TestInvalidHosts()
        {
            AssertInvalidConfig(BuildConfigKey(HostsKeySuffix, WebApp2), "www.foo.com;not/valid");
        }

        [TestMethod]
        public void TestInvalidEmail()
        {
            AssertInvalidConfig(BuildConfigKey(EmailKeySuffix, WebApp1), "mail@");
        }

        [TestMethod]
        public void TestInvalidClientId()
        {
            AssertInvalidConfig(BuildConfigKey(ClientIdKeySuffix, WebApp2), " ");
        }

        [TestMethod]
        public void TestInvalidServicePlanResourceGroup()
        {
            AssertInvalidConfig(BuildConfigKey(ServicePlanResourceGroupKeySuffix, WebApp1), String.Empty);
        }

        [TestMethod]
        public void TestInvalidSiteSlotName()
        {
            AssertInvalidConfig(BuildConfigKey(SiteSlotNameSuffix, WebApp2), " ");
        }

        [TestMethod]
        public void TestInvalidClientSecret()
        {
            var clientSecretKey = BuildConfigKey(ClientSecretKeySuffix, WebApp1);
            m_connectionStrings.Remove(clientSecretKey);
            m_connectionStrings.Add(new ConnectionStringSettings(clientSecretKey, " "));
            AssertExtensions.Throws<ConfigurationErrorsException>(() => m_renewer.Renew());
        }

        [TestMethod]
        public void TestInvalidUseIpBasedSsl()
        {
            AssertInvalidConfig(BuildConfigKey(UseIpBasedSslKeySuffix, WebApp2), String.Empty);
        }

        [TestMethod]
        public void TestInvalidRsaKeyLength()
        {
            AssertInvalidConfig(BuildConfigKey(RsaKeyLengthKeySuffix, WebApp1), "x");
        }

        [TestMethod]
        public void TestInvalidAcmeBaseUri()
        {
            AssertInvalidConfig(BuildConfigKey(AcmeBaseUriKeySuffix, WebApp2), "http:/OnlyOneSlash.com");
        }

        private void AssertInvalidConfig(string key, string value)
        {
            m_appSettings[key] = value;
            AssertExtensions.Throws<ConfigurationErrorsException>(() => m_renewer.Renew());
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