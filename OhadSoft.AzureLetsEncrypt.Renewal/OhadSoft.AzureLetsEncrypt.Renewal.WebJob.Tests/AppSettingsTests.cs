using System;
using System.Collections.Specialized;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings;
using AppSettingsReader = OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings.AppSettingsReader;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests
{
    [TestClass]
    public class AppSettingsTests : RenewalTestBase
    {
        private readonly AppSettingsRenewer m_renewer;
        private readonly NameValueCollection m_appSettings;
        private ConnectionStringSettingsCollection m_connectionStrings;

        public AppSettingsTests()
        {
            m_appSettings = new NameValueCollection
            {
                {"webApps", Webapp1 + ";" + Webapp2},
                {Webapp1 + "-subscriptionId", Subscription1.ToString()},
                {Webapp2 + "-subscriptionId", Subscription2.ToString()},
                {Webapp1 + "-tenantId", Tenant1},
                {Webapp2 + "-tenantId", Tenant2},
                {Webapp1 + "-resourceGroup", ResourceGroup1},
                {Webapp2 + "-resourceGroup", ResourceGroup2},
                {Webapp1 + "-hosts", String.Join(";", Hosts1)},
                {Webapp2 + "-hosts", String.Join(";", Hosts2)},
                {Webapp1 + "-email", Email1},
                {Webapp2 + "-email", Email2},
                {Webapp1 + "-clientId", ClientId1.ToString()},
                {Webapp2 + "-clientId", ClientId2.ToString()},
                {Webapp1 + "-useIpBasedSsl", UseIpBasedSsl1.ToString()},
                {Webapp1 + "-rsaKeyLength", RsaKeyLength1.ToString()},
                {Webapp1 + "-acmeBasedUri", AcmeBaseUrl1.ToString()},
            };
            m_connectionStrings = new ConnectionStringSettingsCollection
            {
                new ConnectionStringSettings(Webapp1 + "-clientSecret", ClientSecret1),
                new ConnectionStringSettings(Webapp2 + "-clientSecret", ClientSecret2)
            };
            m_renewer = new AppSettingsRenewer(CertRenewer, new AppSettingsRenewalParamsReader(new AppSettingsReader(m_appSettings, m_connectionStrings)));
        }

        [TestMethod]
        public void TestSingleWebAppconfig()
        {
            m_appSettings["webApps"] = Webapp1;
            m_renewer.Renew();
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1);
        }

        [TestMethod]
        public void TestDoubleWebAppConfig()
        {
            m_renewer.Renew();
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1, ExpectedPartialRenewalParameters2);
        }
    }
}