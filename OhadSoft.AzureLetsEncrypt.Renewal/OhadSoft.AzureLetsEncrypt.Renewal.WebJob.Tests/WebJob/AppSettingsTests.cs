using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
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
        public const string SubscriptionIdKeySuffix = "-subscriptionId";
        public const string TenantIdKeySuffix = "-tenantId";
        public const string ResourceGroupKeySuffix = "-resourceGroup";
        public const string HostsKeySuffix = "-hosts";
        public const string EmailKeySuffix = "-email";
        public const string ClientIdKeySuffix = "-clientId";
        public const string ClientSecretKeySuffix = "-clientSecret";
        public const string ServicePlanResourceGroupKeySuffix = "-servicePlanResourceGroup";
        public const string SiteSlotNameSuffix = "-siteSlotName";
        public const string UseIpBasedSslKeySuffix = "-useIpBasedSsl";
        public const string RsaKeyLengthKeySuffix = "-rsaKeyLength";
        public const string AcmeBaseUriKeySuffix = "-acmeBaseUri";

        private readonly AppSettingsRenewer m_renewer;

        private readonly NameValueCollection m_appSettings = new NameValueCollection
        {
            { KeyPrefix + WebAppsKey, WebApp1 + ";" + WebApp2 },
            { KeyPrefix + WebApp1 + SubscriptionIdKeySuffix, Subscription1.ToString() },
            { KeyPrefix + WebApp2 + SubscriptionIdKeySuffix, Subscription2.ToString() },
            { KeyPrefix + WebApp1 + TenantIdKeySuffix, Tenant1 },
            { KeyPrefix + WebApp2 + TenantIdKeySuffix, Tenant2 },
            { KeyPrefix + WebApp1 + ResourceGroupKeySuffix, ResourceGroup1 },
            { KeyPrefix + WebApp2 + ResourceGroupKeySuffix, ResourceGroup2 },
            { KeyPrefix + WebApp1 + HostsKeySuffix, String.Join(";", Hosts1) },
            { KeyPrefix + WebApp2 + HostsKeySuffix, String.Join(";", Hosts2) },
            { KeyPrefix + WebApp1 + EmailKeySuffix, Email1 },
            { KeyPrefix + WebApp2 + EmailKeySuffix, Email2 },
            { KeyPrefix + WebApp1 + ClientIdKeySuffix, ClientId1.ToString() },
            { KeyPrefix + WebApp2 + ClientIdKeySuffix, ClientId2.ToString() },
            { KeyPrefix + WebApp1 + ServicePlanResourceGroupKeySuffix, ServicePlanResourceGroup1 },
            { KeyPrefix + WebApp1 + SiteSlotNameSuffix, SiteSlotName1 },
            { KeyPrefix + WebApp1 + UseIpBasedSslKeySuffix, UseIpBasedSsl1.ToString() },
            { KeyPrefix + WebApp1 + RsaKeyLengthKeySuffix, RsaKeyLength1.ToString(CultureInfo.InvariantCulture) },
            { KeyPrefix + WebApp1 + AcmeBaseUriKeySuffix, AcmeBaseUri1.ToString() }
        };

        private readonly ConnectionStringSettingsCollection m_connectionStrings = new ConnectionStringSettingsCollection
        {
            new ConnectionStringSettings(KeyPrefix + WebApp1 + ClientSecretKeySuffix, ClientSecret1),
            new ConnectionStringSettings(KeyPrefix + WebApp2 + ClientSecretKeySuffix, ClientSecret2)
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
            AssertInvalidConfig(WebAppsKey, String.Empty);
        }

        [TestMethod]
        public void TestMissingWebApp()
        {
            AssertInvalidConfig(WebAppsKey, "hello");
        }

        [TestMethod]
        public void TestInvalidSubscriptionId()
        {
            AssertInvalidConfig(WebApp1 + SubscriptionIdKeySuffix, "not a GUID");
        }

        [TestMethod]
        public void TestInvalidTenantId()
        {
            AssertInvalidConfig(WebApp2 + TenantIdKeySuffix, String.Empty);
        }

        [TestMethod]
        public void TestInvalidResourceGroup()
        {
            AssertInvalidConfig(WebApp1 + ResourceGroupKeySuffix, "     ");
        }

        [TestMethod]
        public void TestInvalidHosts()
        {
            AssertInvalidConfig(WebApp2 + HostsKeySuffix, "www.foo.com;not/valid");
        }

        [TestMethod]
        public void TestInvalidEmail()
        {
            AssertInvalidConfig(WebApp1 + EmailKeySuffix, "mail@");
        }

        [TestMethod]
        public void TestInvalidClientId()
        {
            AssertInvalidConfig(WebApp2 + ClientIdKeySuffix, " ");
        }

        [TestMethod]
        public void TestInvalidServicePlanResourceGroup()
        {
            AssertInvalidConfig(WebApp1 + ServicePlanResourceGroupKeySuffix, String.Empty);
        }

        [TestMethod]
        public void TestInvalidSiteSlotName()
        {
            AssertInvalidConfig(WebApp2 + SiteSlotNameSuffix, " ");
        }

        [TestMethod]
        public void TestInvalidClientSecret()
        {
            var clientSecretKey = KeyPrefix + WebApp1 + ClientSecretKeySuffix;
            m_connectionStrings.Remove(clientSecretKey);
            m_connectionStrings.Add(new ConnectionStringSettings(clientSecretKey, " "));
            AssertExtensions.Throws<ConfigurationErrorsException>(() => m_renewer.Renew());
        }

        [TestMethod]
        public void TestInvalidUseIpBasedSsl()
        {
            AssertInvalidConfig(WebApp2 + UseIpBasedSslKeySuffix, String.Empty);
        }

        [TestMethod]
        public void TestInvalidRsaKeyLength()
        {
            AssertInvalidConfig(WebApp1 + RsaKeyLengthKeySuffix, "x");
        }

        [TestMethod]
        public void TestInvalidAcmeBaseUri()
        {
            AssertInvalidConfig(WebApp2 + AcmeBaseUriKeySuffix, "http:/OnlyOneSlash.com");
        }

        private void AssertInvalidConfig(string keyWithoutPrefix, string value)
        {
            m_appSettings[KeyPrefix + keyWithoutPrefix] = value;
            AssertExtensions.Throws<ConfigurationErrorsException>(() => m_renewer.Renew());
        }
    }
}