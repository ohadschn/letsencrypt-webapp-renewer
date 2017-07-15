namespace OhadSoft.AzureLetsEncrypt.Renewal
{
    public interface IRenewalManager
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Params", Justification = "the argument is a parameters class")]
        void Renew(RenewalParameters renewParams);
    }
}