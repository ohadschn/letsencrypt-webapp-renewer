using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Cli
{
    internal interface ICommandlineRenewalParamsReader
    {
        RenewalParameters Read(string[] args);
    }
}