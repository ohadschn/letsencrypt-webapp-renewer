using System.Threading.Tasks;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Email
{
    public interface IEmailNotifier
    {
        Task NotifyAsync(RenewalParameters renewalParams);
    }
}