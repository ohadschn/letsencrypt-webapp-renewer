using OhadSoft.AzureLetsEncrypt.Renewal.Configuration;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public class Renewer
    {
        private readonly ICertRenewer m_certRenewer;
        private readonly IRenewalParamsReader m_renewalParamsReader;

        public Renewer(ICertRenewer certRenewer, IRenewalParamsReader renewalParamsReader)
        {
            m_certRenewer = certRenewer;
            m_renewalParamsReader = renewalParamsReader;
        }

        public void Renew()
        {
            var webAppRenewalInfos = m_renewalParamsReader.Read();
            m_certRenewer.Renew(webAppRenewalInfos);
        }
    }
}