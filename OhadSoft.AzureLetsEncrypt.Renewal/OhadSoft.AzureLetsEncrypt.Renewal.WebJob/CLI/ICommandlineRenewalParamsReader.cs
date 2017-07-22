using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.CLI
{
    internal interface ICommandlineRenewalParamsReader
    {
        RenewalParameters Read(string[] args);
    }
}