using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.CLI
{
    internal class CliRenewer
    {
        private readonly ICertRenewer m_certRenewer;
        private readonly ICommandlineRenewalParamsReader m_renewalParamsReader;

        public CliRenewer(ICertRenewer certRenewer, ICommandlineRenewalParamsReader renewalParamsReader)
        {
            m_certRenewer = certRenewer;
            m_renewalParamsReader = renewalParamsReader;
        }

        public void Renew(string[] args)
        {
            var webAppRenewalInfos = m_renewalParamsReader.Read(args);
            m_certRenewer.Renew(new[] { webAppRenewalInfos });
        }
    }
}