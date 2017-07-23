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
        private const string ValidUseIpBasedSsl = "true";
        private const string ValidRsaKeyLength = "1024";
        private const string ValidAcmeBaseUri = "http://foo.com";

        private readonly string[] m_minimalValidArgs;
        private readonly string[] m_maximalValidArgs;

        public CliTests()
        {
            m_minimalValidArgs = new[]
            {
                Guid.NewGuid().ToString(), "foo-tenant", "foo-resource-group", "foo-webapp", "foo.com;www.bar.com", "foo@gmail.com",
                Guid.NewGuid().ToString(), "foo-secret123"
            };
            m_maximalValidArgs = m_minimalValidArgs.Concat(new[] { ValidUseIpBasedSsl, ValidRsaKeyLength, ValidAcmeBaseUri }).ToArray();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TooFewParametersShouldFail()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            renewer.Renew(m_minimalValidArgs.Take(1).ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TooManyParametersShouldFail()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            renewer.Renew(m_maximalValidArgs.Concat(new []{"foo"}).ToArray());
        }

        [TestMethod]
        public void ProperParametersShouldSucceed()
        {
            var certRenewer = new Mock<ICertRenewer>();
            var renewer = new CliRenewer(certRenewer.Object, new CommandlineRenewalParamsReader());

            var expectedRenewalParams = new RenewalParameters(Guid.Parse(m_minimalValidArgs[0]), m_minimalValidArgs[1], m_minimalValidArgs[2], m_minimalValidArgs[3], m_minimalValidArgs[4].Split(';').ToArray(), m_minimalValidArgs[5], Guid.Parse(m_minimalValidArgs[6]), m_minimalValidArgs[7]);
            renewer.Renew(m_minimalValidArgs);
            certRenewer.Verify(cn => cn.Renew(It.Is<IReadOnlyCollection<RenewalParameters>>(l => l.Count == 1 && l.First().Equals(expectedRenewalParams))));
        }
    }
}