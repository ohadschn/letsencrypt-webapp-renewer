using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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

        public async Task Renew()
        {
            var exceptions = new ConcurrentQueue<Exception>();

            // TODO use ForeachAsync when concurrent renewals are supported: https://github.com/sjkp/letsencrypt-siteextension/issues/161
            foreach (var renewalParams in m_renewalParamsReader.Read())
            {
                Events.RenewalInProgress(renewalParams);
                try
                {
                    await m_renewalManager.Renew(renewalParams);
                    await m_notifier.NotifyAsync(renewalParams);
                }
                catch (Exception e) when (!ExceptionHelper.IsCriticalException(e))
                {
                    Trace.TraceError("Encountered exception: {0}", e);
                    exceptions.Enqueue(e);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("Encountered exception(s) during cert renewal (and/or notification)", exceptions);
            }
        }
    }
}