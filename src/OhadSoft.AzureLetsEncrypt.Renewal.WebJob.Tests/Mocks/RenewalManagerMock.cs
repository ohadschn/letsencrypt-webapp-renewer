using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.Mocks
{
    public class RenewalManagerMock : IRenewalManager
    {
        private readonly ConcurrentQueue<RenewalParameters> m_renewalParams = new ConcurrentQueue<RenewalParameters>();
        public IReadOnlyCollection<RenewalParameters> RenewalParameters => m_renewalParams;

        public Task Renew(RenewalParameters renewalParams)
        {
            m_renewalParams.Enqueue(renewalParams);
            return Task.CompletedTask;
        }
    }
}