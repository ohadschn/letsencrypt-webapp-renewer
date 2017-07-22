using System;
using System.Diagnostics;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.CLI
{
    class Program
    {
        private const int Success = 0;
        private const int ArgumentError = 1;
        private const int UnexpectedError = 2;

        static int Main(string[] args)
        {
            Trace.TraceInformation("Web App SSL renewal CLI started");

            var certRenewer = new CertRenewer(new RenewalManager());
            var renewalParamsReader = new CommandlineRenewalParamsReader();

            try
            {
                var renewalParams = renewalParamsReader.Read(args);
                certRenewer.Renew(new[] {renewalParams});
            }
            catch (ArgumentException e)
            {
                Trace.TraceError("Error parsing arguments: {0}", e);
                PrintUsage();
                return ArgumentError;
            }
            catch (Exception e)
            {
                Trace.TraceError("Unexpected exception: {0}", e);
                return UnexpectedError;
            }

            return Success;
        }

        static void PrintUsage()
        {
            Trace.TraceInformation(
                "Usage: {0}.exe SubscriptionId TenantId ResourceGroup WebApp Hosts Email ClientId ClientSecret [UseIpBasedSsl] [RsaKeyLength] [AcmeBasedUri]", 
                typeof(Program).Assembly.GetName().Name);
            Trace.TraceInformation("'Hosts' is a semicolon-delimited list of host names");
            Trace.TraceInformation("'UseIpBasedSsl' is optional and defaults to false");
            Trace.TraceInformation("'RsaKeyLength' is optional and defaults to 2048");
            Trace.TraceInformation("'AcmeBasedUri' is optional and defaults to https://acme-v01.api.letsencrypt.org/");
            Trace.TraceInformation("Consult the Let's Encrypt documentation for rate limits: https://letsencrypt.org/docs/rate-limits/");
            Trace.TraceInformation("Exit codes: {0} = success, {1} = argument error, {2} = unexpected error", Success, ArgumentError, UnexpectedError);
        }
    }
}