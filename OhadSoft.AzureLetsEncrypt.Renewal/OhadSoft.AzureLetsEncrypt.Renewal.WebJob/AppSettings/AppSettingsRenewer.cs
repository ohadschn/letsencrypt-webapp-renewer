using System;
using System.Threading.Tasks;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings
{
    internal class AppSettingsRenewer
    {
        private readonly IRenewalManager m_renewalManager;
        private readonly IAppSettingsRenewalParamsReader m_renewalParamsReader;

        public AppSettingsRenewer(IRenewalManager renewalManager, IAppSettingsRenewalParamsReader renewalParamsReader)
        {
            m_renewalManager = renewalManager;
            m_renewalParamsReader = renewalParamsReader;
        }

        public void Renew()
        {
            Parallel.ForEach(m_renewalParamsReader.Read(), webAppRenewalInfo =>
            {
                m_renewalManager.Renew(webAppRenewalInfo);
            });
        }
    }
}