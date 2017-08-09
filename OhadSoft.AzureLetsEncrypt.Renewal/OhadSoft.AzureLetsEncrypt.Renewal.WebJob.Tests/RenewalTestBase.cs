using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests
{
    public class RenewalTestBase
    {
        protected static readonly Guid Subscription1 = Guid.Parse("964bbd07-a237-489d-b060-53b027f56d35");
        protected static readonly Guid Subscription2 = Guid.Parse("c6068e1b-27a8-473d-9113-d443d1ae8859");
        protected const string Tenant1 = "foo-tenant";
        protected const string Tenant2 = "bar-tenant";
        protected const string ResourceGroup1 = "foo-resource-group";
        protected const string ResourceGroup2 = "bar-resource-group";
        protected const string Webapp1 = "fooApp";
        protected const string Webapp2 = "barApp";
        protected static readonly IReadOnlyList<string> Hosts1 = new[] { "www.foo.com" };
        protected static readonly IReadOnlyList<string> Hosts2 = new[] { "www.bar.com", "bar.com" };
        protected const string Email1 = "foo@gmail.com";
        protected const string Email2 = "bar@outlook.com";
        protected static readonly Guid ClientId1 = Guid.Parse("89e5c1ee-a37b-4af7-89fc-217a4b99652c");
        protected static readonly Guid ClientId2 = Guid.Parse("618a929b-d9c9-4ec1-b8dc-66f55d949d52");
        protected const string ClientSecret1 = "foo-secret123";
        protected const string ClientSecret2 = "bar-verySecret321";
        protected const bool UseIpBasedSsl1 = true;
        protected const bool UseIpBasedSsl2 = false;
        protected const int RsaKeyLength1 = 1024;
        protected const int RsaKeyLength2 = 4096;
        protected static readonly Uri AcmeBaseUrl1 = new Uri("http://foo.example.com");
        protected static readonly Uri AcmeBaseUrl2 = new Uri("http://bar.example.com");

        protected static readonly RenewalParameters ExpectedFullRenewalParameters1 = new RenewalParameters(
            Subscription1, 
            Tenant1, 
            ResourceGroup1,
            Webapp1, 
            Hosts1, 
            Email1, 
            ClientId1, 
            ClientSecret1, 
            UseIpBasedSsl1, 
            RsaKeyLength1, 
            AcmeBaseUrl1);

        protected static RenewalParameters ExpectedPartialRenewalParameters1 = new RenewalParameters(
            Subscription1, 
            Tenant1, 
            ResourceGroup1, 
            Webapp1, 
            Hosts1, 
            Email1, 
            ClientId1, 
            ClientSecret1);

        protected static readonly RenewalParameters ExpectedFullRenewalParameters2 = new RenewalParameters(
            Subscription2,
            Tenant2,
            ResourceGroup2,
            Webapp2,
            Hosts2,
            Email2,
            ClientId2,
            ClientSecret2,
            UseIpBasedSsl2,
            RsaKeyLength2,
            AcmeBaseUrl2);

        protected static RenewalParameters ExpectedPartialRenewalParameters2 = new RenewalParameters(
            Subscription2,
            Tenant2,
            ResourceGroup2,
            Webapp2,
            Hosts2,
            Email2,
            ClientId2,
            ClientSecret2);

        private readonly Mock<ICertRenewer> m_certRenewerMock = new Mock<ICertRenewer>();
        protected ICertRenewer CertRenewer => m_certRenewerMock.Object;

        protected void VerifySuccessfulRenewal(RenewalParameters renewalParameters)
        {
            m_certRenewerMock.Verify(cn => cn.Renew(It.Is<IReadOnlyCollection<RenewalParameters>>(l => l.Count == 1 && l.First().Equals(renewalParameters))));
        }
    }
}