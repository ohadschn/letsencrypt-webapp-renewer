using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.CLI
{
    internal interface ICommandlineRenewalParamsReader
    {
        RenewalParameters Read(string[] args);
    }
}