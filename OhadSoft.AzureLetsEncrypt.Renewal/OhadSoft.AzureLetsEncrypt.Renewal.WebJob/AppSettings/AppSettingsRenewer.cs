using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Email;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Telemetry;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Util;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings
{
    internal class AppSettingsRenewer
    {
        private readonly IRenewalManager m_renewalManager;
        private readonly IAppSettingsRenewalParamsReader m_renewalParamsReader;
        private readonly IEmailNotifier m_notifier;

        public AppSettingsRenewer(IRenewalManager renewalManager, IAppSettingsRenewalParamsReader renewalParamsReader, IEmailNotifier notifier)
        {
            m_renewalManager = renewalManager;
            m_renewalParamsReader = renewalParamsReader;
            m_notifier = notifier;
        }

        public void Renew()
        {
            var exceptions = new ConcurrentQueue<Exception>();
            Parallel.ForEach(m_renewalParamsReader.Read(), renewalParams =>
            {
                try
                {
                    m_renewalManager.Renew(renewalParams);
                    m_notifier.NotifyAsync(renewalParams).Wait(); // TODO use ForeachAsync when renewal manager supports it
                }
                catch (Exception e) when (!ExceptionHelper.IsCriticalException(e))
                {
                    Console.WriteLine("ERROR: Encountered exception: " + e);
                    exceptions.Enqueue(e);
                }
            });

            if (exceptions.Count > 0)
            {
                throw new AggregateException("Encountered exception(s) during cert renewal (and/or notification)", exceptions);
            }
        }
    }
}