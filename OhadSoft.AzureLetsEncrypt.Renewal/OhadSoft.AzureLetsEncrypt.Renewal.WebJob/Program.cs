using System;
using System.Diagnostics;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Configuration;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob
{
    internal static class Program
    {
        private static void Main()
        {
            Trace.TraceInformation(
                "Web App SSL renewal job ({0}) started [Run ID: {1}]",
                Environment.GetEnvironmentVariable("WEBJOBS_NAME"),
                Environment.GetEnvironmentVariable("WEBJOBS_RUN_ID "));

            // TODO unify with webjob (using something like fromConfig switch, or maybe detect if running in webjob context)
            var certRenewer = new CertRenewer(new RenewalManager());
            var renewalParamsReader = new AppSettingsRenewalParamsReader(new AppSettingsReader());

            try
            {
                var renewalParams = renewalParamsReader.Read();
                certRenewer.Renew(renewalParams);
            }
            catch (Exception e)
            {
                Trace.TraceError("Unexpected exception: {0}", e);
                throw; // we want the webjob to fail
            }
        }
    }
}