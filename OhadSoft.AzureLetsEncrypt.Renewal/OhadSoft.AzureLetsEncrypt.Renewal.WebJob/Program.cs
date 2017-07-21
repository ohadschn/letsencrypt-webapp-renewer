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

            var renewer = new Renewer(new CertRenewer(new RenewalManager()), new AppSettingsRenewalParamsReader(new AppSettingsReader()));

            try
            {
                renewer.Renew();
            }
            catch (Exception e)
            {
                Trace.TraceError("Unexpected exception: {0}", e);
                throw; // we want the webjob to fail
            }
        }
    }
}