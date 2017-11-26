using System;
using System.Configuration;
using System.Diagnostics;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Cli;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Email;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Telemetry;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Util;
using static System.FormattableString;
using AppSettingsReader = OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings.AppSettingsReader;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob
{
    internal static class Program
    {
        private const string DisableTelemetryEnvVarNAme = "LETSENCRYPT_DISABLE_TELEMETRY";
        private const string VerboseOutputEnvVarName = "LETSENCRYPT_VERBOSE";

        private static int Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable(DisableTelemetryEnvVarNAme) == null)
            {
                TelemetryHelper.Setup();
            }
            else
            {
                Trace.TraceInformation("{0} environment variable detected - telemetry disabled", DisableTelemetryEnvVarNAme);
            }

            try
            {
                var webjobName = Environment.GetEnvironmentVariable("WEBJOBS_NAME");
                return webjobName != null ? WebJobMain(webjobName) : CliMain(args);
            }
            finally
            {
                TelemetryHelper.Client.Flush();
            }
        }

        private static int WebJobMain(string webjobName)
        {
            var startTicks = Environment.TickCount;
            Events.WebJobRenewalStarted(webjobName);

            Trace.TraceInformation("Web App SSL renewal job ({0}) started", webjobName);
            var renewr = new AppSettingsRenewer(
                new RenewalManager(),
                new AppSettingsRenewalParamsReader(new AppSettingsReader(ConfigurationManager.AppSettings, ConfigurationManager.ConnectionStrings)),
                new SendGridNotifier(ConfigurationManager.ConnectionStrings[AppSettingsRenewalParamsReader.KeyPrefix + "SendGridApiKey"]?.ConnectionString));
            try
            {
                renewr.Renew().GetAwaiter().GetResult();
            }
            catch (Exception e) when (!ExceptionHelper.IsCriticalException(e))
            {
                Trace.TraceError("Unexpected exception: {0}", e);
                throw; // we want the webjob to fail
            }

            Events.WebjobRenewalCompleted(webjobName, startTicks);
            return ExitCodes.Success;
        }

        private static int CliMain(string[] args)
        {
            var startTicks = Environment.TickCount;
            Events.CliRenewalStarted(args);

            Trace.TraceInformation("Web App SSL renewal CLI started, parameters: {0}", string.Join(", ", args));
            var renewer = new CliRenewer(new RenewalManager(), new CommandlineRenewalParamsReader());

            try
            {
                renewer.Renew(args);
            }
            catch (ArgumentValidationException e)
            {
                Trace.TraceError(e.Message);
                Console.WriteLine(e.HelpText);
                TelemetryHelper.Client.TrackException(e);
            }
            catch (ArgumentException e)
            {
                Trace.TraceError(
                    "Could not parse arguments: {0}{1}{2}",
                    e.Message,
                    Environment.NewLine,
                    Environment.GetEnvironmentVariable(VerboseOutputEnvVarName) != null
                        ? e.ToString()
                        : Invariant($"(To see the full exception set the {VerboseOutputEnvVarName} environment variable to any non-empty value)"));

                TelemetryHelper.Client.TrackException(e);

                return ExitCodes.ArgumentError;
            }
            catch (Exception e) when (!ExceptionHelper.IsCriticalException(e))
            {
                Trace.TraceError("Unexpected exception: {0}", e);
                TelemetryHelper.Client.TrackException(e);
                return ExitCodes.UnexpectedException;
            }

            Events.CliRenewalCompleted(startTicks);
            return ExitCodes.Success;
        }
    }
}