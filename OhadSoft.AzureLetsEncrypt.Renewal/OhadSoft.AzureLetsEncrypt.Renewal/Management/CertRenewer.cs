using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public class CertRenewer : ICertRenewer
    {
        private readonly IRenewalManager m_renewalManager;

        public CertRenewer(IRenewalManager renewalManager)
        {
            m_renewalManager = renewalManager;
        }

        public void Renew(IReadOnlyCollection<RenewalParameters> webAppRenewalParams)
        {
            if (webAppRenewalParams == null)
            {
                throw new ArgumentNullException(nameof(webAppRenewalParams));
            }

            if (webAppRenewalParams.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(webAppRenewalParams), "Empty renewal parameter collection");
            }

            Parallel.ForEach(webAppRenewalParams, webAppRenewalInfo =>
            {
                Trace.TraceInformation("Renewing SSL cert for Web App '{0}'...", webAppRenewalInfo.WebApp);
                m_renewalManager.Renew(webAppRenewalInfo);
                Trace.TraceInformation("Completed renewal of SSL cert for Web App '{0}'", webAppRenewalInfo.WebApp);
            });
        }
    }
}