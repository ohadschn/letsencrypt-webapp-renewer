using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Configuration;
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
        public const string UseIpBasedSslKeySuffix = "-useIpBasedSsl";
        public const string RsaKeyLengthKeySuffix = "-rsaKeyLength";
        public const string AcmeBaseUriKeySuffix = "-acmeBaseUri";

        private readonly AppSettingsRenewer m_renewer;

        private readonly NameValueCollection m_appSettings = new NameValueCollection
        {
            {KeyPrefix + WebAppsKey, Webapp1 + ";" + Webapp2},
            {KeyPrefix + Webapp1 + SubscriptionIdKeySuffix, Subscription1.ToString()},
            {KeyPrefix + Webapp2 + SubscriptionIdKeySuffix, Subscription2.ToString()},
            {KeyPrefix + Webapp1 + TenantIdKeySuffix, Tenant1},
            {KeyPrefix + Webapp2 + TenantIdKeySuffix, Tenant2},
            {KeyPrefix + Webapp1 + ResourceGroupKeySuffix, ResourceGroup1},
            {KeyPrefix + Webapp2 + ResourceGroupKeySuffix, ResourceGroup2},
            {KeyPrefix + Webapp1 + HostsKeySuffix, String.Join(";", Hosts1)},
            {KeyPrefix + Webapp2 + HostsKeySuffix, String.Join(";", Hosts2)},
            {KeyPrefix + Webapp1 + EmailKeySuffix, Email1},
            {KeyPrefix + Webapp2 + EmailKeySuffix, Email2},
            {KeyPrefix + Webapp1 + ClientIdKeySuffix, ClientId1.ToString()},
            {KeyPrefix + Webapp2 + ClientIdKeySuffix, ClientId2.ToString()},
            {KeyPrefix + Webapp1 + UseIpBasedSslKeySuffix, UseIpBasedSsl1.ToString()},
            {KeyPrefix + Webapp1 + RsaKeyLengthKeySuffix, RsaKeyLength1.ToString()},
            {KeyPrefix + Webapp1 + AcmeBaseUriKeySuffix, AcmeBaseUri1.ToString()}
        };

        private readonly ConnectionStringSettingsCollection m_connectionStrings = new ConnectionStringSettingsCollection
        {
            new ConnectionStringSettings(KeyPrefix + Webapp1 + ClientSecretKeySuffix, ClientSecret1),
            new ConnectionStringSettings(KeyPrefix + Webapp2 + ClientSecretKeySuffix, ClientSecret2)
        };

        readonly Mock<IEmailNotifier> m_emailNotifier = new Mock<IEmailNotifier>();
        readonly ConcurrentQueue<RenewalParameters> m_notificationRenewalParameterses = new ConcurrentQueue<RenewalParameters>();

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
        public void TestSingleWebAppconfig()
        {
            m_appSettings[KeyPrefix + WebAppsKey] = Webapp1;
            m_renewer.Renew();
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1);
            VerifySuccessfulRenewal(new[] {ExpectedFullRenewalParameters1}, m_notificationRenewalParameterses);
        }

        [TestMethod]
        public void TestDoubleWebAppConfig()
        {
            m_renewer.Renew();
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1, ExpectedPartialRenewalParameters2);
            VerifySuccessfulRenewal(new []{ ExpectedFullRenewalParameters1, ExpectedPartialRenewalParameters2 }, m_notificationRenewalParameterses);
        }

        [TestMethod]
        public void TestInvalidWebApps()
        {
            AssertInvalidConfig(WebAppsKey, "");
        }

        [TestMethod]
        public void TestMissingWebApp()
        {
            AssertInvalidConfig(WebAppsKey, "hello");
        }

        [TestMethod]
        public void TestInvalidSubscriptionId()
        {
            AssertInvalidConfig(Webapp1 + SubscriptionIdKeySuffix, "not a GUID");
        }

        [TestMethod]
        public void TestInvalidTenantId()
        {
            AssertInvalidConfig(Webapp2 + TenantIdKeySuffix, "");
        }

        [TestMethod]
        public void TestInvalidResourceGroup()
        {
            AssertInvalidConfig(Webapp1 + ResourceGroupKeySuffix, "     ");
        }

        [TestMethod]
        public void TestInvalidHosts()
        {
            AssertInvalidConfig(Webapp2 + HostsKeySuffix, "www.foo.com;not/valid");
        }

        [TestMethod]
        public void TestInvalidEmail()
        {
            AssertInvalidConfig(Webapp1 + EmailKeySuffix, "mail@");
        }

        [TestMethod]
        public void TestInvalidClientId()
        {
            AssertInvalidConfig(Webapp2 + ClientIdKeySuffix, " ");
        }

        [TestMethod]
        public void TestInvalidClientSecret()
        {
            var clientSecretKey = KeyPrefix + Webapp1 + ClientSecretKeySuffix;
            m_connectionStrings.Remove(clientSecretKey);
            m_connectionStrings.Add(new ConnectionStringSettings(clientSecretKey, " "));
            AssertExtensions.Throws<ConfigurationErrorsException>(() => m_renewer.Renew());
        }

        [TestMethod]
        public void TestInvalidUseIpBasedSsl()
        {
            AssertInvalidConfig(Webapp2 + UseIpBasedSslKeySuffix, "");
        }

        [TestMethod]
        public void TestInvalidRsaKeyLength()
        {
            AssertInvalidConfig(Webapp1 + RsaKeyLengthKeySuffix, "x");
        }

        [TestMethod]
        public void TestInvalidAcmeBaseUri()
        {
            AssertInvalidConfig(Webapp2 + AcmeBaseUriKeySuffix, "http:/OnlyOneSlash.com");
        }

        private void AssertInvalidConfig(string keyWithoutPrefix, string value)
        {
            m_appSettings[KeyPrefix +  keyWithoutPrefix] = value;
            AssertExtensions.Throws<ConfigurationErrorsException>(() => m_renewer.Renew());
        }
    }
}