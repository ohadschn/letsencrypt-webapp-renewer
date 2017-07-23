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

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TooFewParameters()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            renewer.Renew(FullValidArgs.Take(7).ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TooManyParameters()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            renewer.Renew(FullValidArgs.Concat(new []{"foo"}).ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidSubscriptionId()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMaximalValidArgs();
            args[0] = "e004af7e-50be-41af-ac1a-hello";
            renewer.Renew(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidTenant()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMinimalValidArgs();
            args[1] = "";
            renewer.Renew(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidResourceGroup()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMaximalValidArgs();
            args[2] = " ";
            renewer.Renew(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidWebApp()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMaximalValidArgs();
            args[3] = "     ";
            renewer.Renew(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidHosts()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMinimalValidArgs();
            args[4] = "     ";
            renewer.Renew(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidEmail()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMaximalValidArgs();
            args[5] = "@notAnEmail";
            renewer.Renew(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidClientId()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMinimalValidArgs();
            args[6] = Guid.Empty.ToString();
            renewer.Renew(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidSecret()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMaximalValidArgs();
            args[7] = "";
            renewer.Renew(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidUseIpBasedSsl()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMaximalValidArgs();
            args[8] = "notTrueOrFalse";
            renewer.Renew(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidRsaKeyLength()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMaximalValidArgs();
            args[9] = "-1";
            renewer.Renew(args);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidAcmeBasedUrl()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var args = GetMaximalValidArgs();
            args[10] = "/not/absolute";
            renewer.Renew(args);
        }

        [TestMethod]
        public void MinimalProperParametersShouldSucceed()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var expectedRenewalParams = new RenewalParameters(FooSubscription, FooTenant, FooResourceGroup, FooWebapp, Hosts, FooEmail, FooClient, FooSecret);

            new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader()).Renew(GetMinimalValidArgs());
            certRenewer.Verify(cn => cn.Renew(It.Is<IReadOnlyCollection<RenewalParameters>>(l => l.Count == 1 && l.First().Equals(expectedRenewalParams))));
        }

        [TestMethod]
        public void MaximalProperParametersShouldSucceed()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var expectedRenewalParams = 
                new RenewalParameters(FooSubscription, FooTenant, FooResourceGroup, FooWebapp, Hosts, FooEmail,FooClient, FooSecret, UseIpBasedSsl, RsaKeyLength, AcmeBaseUrl);

            new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader()).Renew(GetMaximalValidArgs());
            certRenewer.Verify(cn => cn.Renew(It.Is<IReadOnlyCollection<RenewalParameters>>(l => l.Count == 1 && l.First().Equals(expectedRenewalParams))));
        }
    }
}