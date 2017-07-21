using System.Collections.Generic;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public interface ICertRenewer
    {
        void Renew(IReadOnlyCollection<RenewalParameters> webAppRenewalParams);
    }
}