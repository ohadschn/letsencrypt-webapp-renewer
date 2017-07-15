namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob
{
    public interface IRenewer
    {
        void RenewWebAppCertFromConfiguration(IRenewalManager renewalManager, IConfigurationHelper configHelper);
    }
}