using OhadSoft.AzureLetsEncrypt.Renewal.Configuration;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public interface IRenewer
    {
        void RenewWebAppCertFromConfiguration(IRenewalManager renewalManager, IConfigurationReader configReader);
    }
}