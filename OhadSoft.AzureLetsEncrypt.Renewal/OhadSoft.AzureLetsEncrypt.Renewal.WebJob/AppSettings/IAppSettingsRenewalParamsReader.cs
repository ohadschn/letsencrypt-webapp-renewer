using System.Collections.Generic;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings
{
    internal interface IAppSettingsRenewalParamsReader
    {
        IReadOnlyCollection<RenewalParameters> Read();
    }
}