using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Telemetry;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Cli
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
            var renewalParameters = m_renewalParamsReader.Read(args);
            Events.RenewalInProgress(renewalParameters);
            m_renewalManager.Renew(renewalParameters).GetAwaiter().GetResult();
        }
    }
}