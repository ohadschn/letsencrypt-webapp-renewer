using System.Collections.Generic;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Configuration
{
    internal interface IRenewalParamsReader
    {
        IReadOnlyCollection<RenewalParameters> Read();
    }
}