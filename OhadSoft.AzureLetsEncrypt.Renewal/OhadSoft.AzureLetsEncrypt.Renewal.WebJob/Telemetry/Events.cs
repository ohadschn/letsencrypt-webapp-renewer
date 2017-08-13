using System;
using System.Collections.Generic;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Telemetry
{
    internal static class Events
    {
        public static void WebJobRenewalStarted(string webjobName)
        {
            if (!TelemetryHelper.TelemetryInitialized)
            {
                return;
            }

            if (webjobName == null)
            {
                TelemetryHelper.TrackInternalError(new ArgumentNullException(nameof(webjobName)));
                return;
            }

            TelemetryHelper.Client.TrackEvent("WebJobRenewalStarted", new Dictionary<string, string> { { "webjobName", TelemetryHelper.Hash(webjobName) } });
        }

        public static void WebjobRenewalCompleted(string webjobName, int startTickCount)
        {
            if (!TelemetryHelper.TelemetryInitialized)
            {
                return;
            }

            if (webjobName == null)
            {
                TelemetryHelper.TrackInternalError(new ArgumentNullException(nameof(webjobName)));
                return;
            }

            int durationMs = Environment.TickCount - startTickCount;

            TelemetryHelper.Client.TrackEvent(
                "WebJobRenewalCompleted",
                new Dictionary<string, string> { { "webjobName", TelemetryHelper.Hash(webjobName) } },
                new Dictionary<string, double> { { "durationMs", durationMs } });
        }

        public static void CliRenewalStarted(string[] args)
        {
            if (!TelemetryHelper.TelemetryInitialized)
            {
                return;
            }

            if (args == null)
            {
                TelemetryHelper.TrackInternalError(new ArgumentNullException(nameof(args)));
                return;
            }

            TelemetryHelper.Client.TrackEvent("CliRenewalStarted", null, new Dictionary<string, double> { { "argCount", args.Length } });
        }

        public static void CliRenewalCompleted(int startTickCount)
        {
            if (!TelemetryHelper.TelemetryInitialized)
            {
                return;
            }

            int durationMs = Environment.TickCount - startTickCount;
            TelemetryHelper.Client.TrackEvent("CliRenewalCompleted", null, new Dictionary<string, double> { { "durationMs", durationMs } });
        }

        public static void RenewalInProgress(RenewalParameters renewalParams)
        {
            if (!TelemetryHelper.TelemetryInitialized)
            {
                return;
            }

            if (renewalParams == null)
            {
                TelemetryHelper.TrackInternalError(new ArgumentNullException(nameof(renewalParams)));
                return;
            }

            foreach (var host in renewalParams.Hosts) // normalize events so only one per host is fired
            {
                TelemetryHelper.Client.TrackEvent(
                    "CertRenewal",
                    new Dictionary<string, string>
                    {
                        { "subscriptionId", renewalParams.SubscriptionId.ToString() },
                        { "tenantId", renewalParams.TenantId },
                        { "resourceGroup", TelemetryHelper.Hash(renewalParams.ResourceGroup) },
                        { "webApp", TelemetryHelper.Hash(renewalParams.WebApp) },
                        { "host", TelemetryHelper.Hash(host) },
                        { "email", TelemetryHelper.Hash(renewalParams.Email) },
                        { "clientId", renewalParams.ClientId.ToString() },
                        { "useIpBasedSsl", renewalParams.UseIpBasedSsl.ToString() },
                        { "acmeBaseUri", renewalParams.AcmeBaseUri == null ? "[DEFAULT]" : TelemetryHelper.Hash(renewalParams.AcmeBaseUri.ToString()) }
                    },
                    new Dictionary<string, double>
                    {
                        { "rsaKeyLength", renewalParams.RsaKeyLength }
                    });
            }
        }
    }
}