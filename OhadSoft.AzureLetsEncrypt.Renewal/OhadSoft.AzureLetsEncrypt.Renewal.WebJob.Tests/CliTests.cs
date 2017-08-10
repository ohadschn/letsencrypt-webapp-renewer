using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.CLI;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests
{
    [TestClass]
    public class CliTests : RenewalTestBase
    {
        private static readonly IReadOnlyCollection<string> FullValidArgs = new[]
        {
            Subscription1.ToString(), Tenant1, ResourceGroup1, Webapp1, String.Join(";", Hosts1),
            Email1, ClientId1.ToString(), ClientSecret1, UseIpBasedSsl1.ToString(), RsaKeyLength1.ToString(), AcmeBaseUri1.ToString()
        };

        private static string[] GetMaximalValidArgs() => FullValidArgs.ToArray();
        private static string[] GetMinimalValidArgs() => FullValidArgs.Take(8).ToArray();

        private readonly CliRenewer m_renewer;

        public CliTests()
        {
            m_renewer = new CliRenewer(RenewalManager, new CommandlineRenewalParamsReader());
        }

        [TestMethod]
        public void TooFewParameters()
        {
            var strings = FullValidArgs.Take(7).ToArray();
            AssertExtensions.Throws<ArgumentException>(() => m_renewer.Renew(strings));
        }

        [TestMethod]
        public void TooManyParameters()
        {
            var strings = FullValidArgs.Concat(new []{"foo"}).ToArray();
            AssertExtensions.Throws<ArgumentException>(() => m_renewer.Renew(strings));
        }

        [TestMethod]
        public void InvalidSubscriptionId()
        {
            TestInvalidParameter(0, "e004af7e-50be-41af-ac1a-hello");
        }

        [TestMethod]
        public void InvalidTenant()
        {
            TestInvalidParameter(1, "");
        }

        [TestMethod]
        public void InvalidResourceGroup()
        {
            TestInvalidParameter(2, " ");
        }

        [TestMethod]
        public void InvalidWebApp()
        {
            TestInvalidParameter(3, "     ");
        }

        [TestMethod]
        public void InvalidHosts()
        {
            TestInvalidParameter(4, "/");
        }

        [TestMethod]
        public void InvalidEmail()
        {
            TestInvalidParameter(5, "@notAnEmail");
        }

        [TestMethod]
        public void InvalidClientId()
        {
            TestInvalidParameter(6, Guid.Empty.ToString());
        }

        [TestMethod]
        public void InvalidSecret()
        {
            TestInvalidParameter(7, "");
        }

        [TestMethod]
        public void InvalidUseIpBasedSsl()
        {
            TestInvalidParameter(8, "notTrueOrFalse");
        }

        [TestMethod]
        public void InvalidRsaKeyLength()
        {
            TestInvalidParameter(9, "-1");
        }

        [TestMethod]
        public void InvalidAcmeBaseUri()
        {
            TestInvalidParameter(10, "www.nohttp.com");
        }

        private void TestInvalidParameter(int index, string value)
        {
            var args = index <= 7 ? GetMinimalValidArgs() : GetMaximalValidArgs();
            args[index] = value;
            AssertExtensions.Throws<ArgumentException>(() => m_renewer.Renew(args));
        }

        [TestMethod]
        public void MinimalProperParametersShouldSucceed()
        {
            m_renewer.Renew(GetMinimalValidArgs());
            VerifySuccessfulRenewal(ExpectedPartialRenewalParameters1);
        }

        [TestMethod]
        public void MaximalProperParametersShouldSucceed()
        {
            m_renewer.Renew(GetMaximalValidArgs());
            VerifySuccessfulRenewal(ExpectedFullRenewalParameters1);
        }
    }
}