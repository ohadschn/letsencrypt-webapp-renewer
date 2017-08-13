namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public interface IRenewalManager
    {
        void Renew(RenewalParameters renewParams);
    }
}