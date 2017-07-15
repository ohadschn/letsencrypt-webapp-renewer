namespace OhadSoft.AzureLetsEncrypt.Renewal
{
    public interface IRenewalManager
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Params")]
        void Renew(RenewalParameters renewParams);
    }
}