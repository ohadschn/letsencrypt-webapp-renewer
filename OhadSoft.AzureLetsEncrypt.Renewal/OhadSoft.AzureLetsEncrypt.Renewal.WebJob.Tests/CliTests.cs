using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.CLI;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests
{
    [TestClass]
    public class CliTests
    {
        private static readonly Guid FooSubscription = Guid.Parse("a0c04c9b-a1ca-4455-9f10-d1945175c1df");
        private const string FooTenant = "foo-tenant";
        private const string FooResourceGroup = "foo-resource-group";
        private const string FooWebapp = "foo-webapp";
        private static readonly IReadOnlyList<string> Hosts = new[] {"foo.com", "www.bar.com"};
        private const string FooEmail = "foo@gmail.com";
        private static readonly Guid FooClient = Guid.Parse("2442cca0-0145-419b-8747-259e815fa011");
        private const string FooSecret = "foo-secret123";
        private const bool UseIpBasedSsl = true;
        private const int RsaKeyLength = 1024;
        private static readonly Uri AcmeBaseUrl = new Uri("http://foo.com");

        private static readonly IReadOnlyCollection<string> FullValidArgs = new[]
        {
            FooSubscription.ToString(), FooTenant, FooResourceGroup, FooWebapp, String.Join(";", Hosts),
            FooEmail, FooClient.ToString(), FooSecret, UseIpBasedSsl.ToString(), RsaKeyLength.ToString(), AcmeBaseUrl.ToString()
        };

        private static string[] GetMaximalValidArgs() => FullValidArgs.ToArray();
        private static string[] GetMinimalValidArgs() => FullValidArgs.Take(8).ToArray();

        private readonly Mock<ICertRenewer> m_certRenewer;
        private readonly CliRenewer m_renewer;

        public CliTests()
        {
            m_certRenewer = new Mock<ICertRenewer>();
            m_renewer = new CliRenewer(m_certRenewer.Object, new CommandlineRenewalParamsReader());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TooFewParameters()
        {
            m_renewer.Renew(FullValidArgs.Take(7).ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TooManyParameters()
        {
            m_renewer.Renew(FullValidArgs.Concat(new []{"foo"}).ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidSubscriptionId()
        {
            TestInvalidParameter(0, "e004af7e-50be-41af-ac1a-hello");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidTenant()
        {
            TestInvalidParameter(1, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidResourceGroup()
        {
            TestInvalidParameter(2, " ");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidWebApp()
        {
            TestInvalidParameter(3, "     ");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidHosts()
        {
            TestInvalidParameter(4, "     ");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidEmail()
        {
            TestInvalidParameter(5, "@notAnEmail");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidClientId()
        {
            TestInvalidParameter(6, Guid.Empty.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidSecret()
        {
            TestInvalidParameter(7, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidUseIpBasedSsl()
        {
            TestInvalidParameter(8, "notTrueOrFalse");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidRsaKeyLength()
        {
            TestInvalidParameter(9, "-1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidAcmeBasedUrl()
        {
            TestInvalidParameter(10, "/not/absolute");
        }

        private void TestInvalidParameter(int index, string value)
        {
            var args = index <= 7 ? GetMinimalValidArgs() : GetMaximalValidArgs();
            args[index] = value;
            m_renewer.Renew(args);
        }

        [TestMethod]
        public void MinimalProperParametersShouldSucceed()
        {
            var expectedRenewalParams = new RenewalParameters(FooSubscription, FooTenant, FooResourceGroup, FooWebapp, Hosts, FooEmail, FooClient, FooSecret);

            new CliRenewer(m_certRenewer.Object, new CommandlineRenewalParamsReader()).Renew(GetMinimalValidArgs());
            m_certRenewer.Verify(cn => cn.Renew(It.Is<IReadOnlyCollection<RenewalParameters>>(l => l.Count == 1 && l.First().Equals(expectedRenewalParams))));
        }

        [TestMethod]
        public void MaximalProperParametersShouldSucceed()
        {
            var expectedRenewalParams = 
                new RenewalParameters(FooSubscription, FooTenant, FooResourceGroup, FooWebapp, Hosts, FooEmail,FooClient, FooSecret, UseIpBasedSsl, RsaKeyLength, AcmeBaseUrl);

            new CliRenewer(m_certRenewer.Object, new CommandlineRenewalParamsReader()).Renew(GetMaximalValidArgs());
            m_certRenewer.Verify(cn => cn.Renew(It.Is<IReadOnlyCollection<RenewalParameters>>(l => l.Count == 1 && l.First().Equals(expectedRenewalParams))));
        }
    }
}