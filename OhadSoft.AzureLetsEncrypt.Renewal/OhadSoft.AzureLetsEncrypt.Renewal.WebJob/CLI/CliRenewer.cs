using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.CLI
{
    internal class CliRenewer
    {
        private readonly IRenewalManager m_renewalManager;
        private readonly ICommandlineRenewalParamsReader m_renewalParamsReader;

        public CliRenewer(IRenewalManager renewalManager, ICommandlineRenewalParamsReader renewalParamsReader)
        {
            m_renewalManager = renewalManager;
            m_renewalParamsReader = renewalParamsReader;
        }

        public void Renew(string[] args)
        {
            m_renewalManager.Renew(m_renewalParamsReader.Read(args));
        }
    }
}