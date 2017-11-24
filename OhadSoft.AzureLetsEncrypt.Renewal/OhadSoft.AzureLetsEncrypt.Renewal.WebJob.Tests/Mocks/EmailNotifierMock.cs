using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Email;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.Mocks
{
    public class EmailNotifierMock : IEmailNotifier
    {
        private readonly ConcurrentQueue<RenewalParameters> m_renewalParams = new ConcurrentQueue<RenewalParameters>();
        public IReadOnlyCollection<RenewalParameters> RenewalParameters => m_renewalParams;

        public Task NotifyAsync(RenewalParameters renewalParams)
        {
            m_renewalParams.Enqueue(renewalParams);
            return Task.CompletedTask;
        }
    }
}