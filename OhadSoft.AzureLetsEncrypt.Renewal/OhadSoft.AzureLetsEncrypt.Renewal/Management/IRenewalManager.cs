using System.Threading.Tasks;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public interface IRenewalManager
    {
        Task Renew(RenewalParameters renewalParams);
    }
}