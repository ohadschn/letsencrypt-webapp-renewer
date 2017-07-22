using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings
{
    internal class AppSettingsRenewer
    {
        private readonly ICertRenewer m_certRenewer;
        private readonly IAppSettingsRenewalParamsReader m_renewalParamsReader;

        public AppSettingsRenewer(ICertRenewer certRenewer, IAppSettingsRenewalParamsReader renewalParamsReader)
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