using System.Collections.Generic;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Configuration
{
    public interface IRenewalParamsReader
    {
        IReadOnlyCollection<RenewalParameters> Read();
    }
}