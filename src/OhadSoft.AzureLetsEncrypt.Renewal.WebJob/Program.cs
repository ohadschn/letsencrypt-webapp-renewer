using System;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Cli;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Email;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Exceptions;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Telemetry;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Util;
using AppSettingsReader = OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings.AppSettingsReader;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob
{
    internal static class Program
    {
        private const string DisableTelemetryEnvVarName = "LETSENCRYPT_DISABLE_TELEMETRY";

        [SuppressMessage("Sonar", "S4210:Windows Forms entry points should be marked with STAThread", Justification = "Transitive from System.Web, not used")]
        private static int Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable(DisableTelemetryEnvVarName) == null)
            {
                TelemetryHelper.Setup();
            }
            else
            {
                Trace.TraceInformation("{0} environment variable detected - telemetry disabled", DisableTelemetryEnvVarName);
            }

            if (Environment.GetEnvironmentVariable("DEBUG_MODE") != null)
            {
                Trace.TraceInformation("*** DEBUG MODE ENABLED (all exceptions will crash process) ***");
                ExceptionHelper.DebugMode = true;
            }

            try
            {
                var webjobName = Environment.GetEnvironmentVariable("WEBJOBS_NAME");
                return webjobName != null ? WebJobMain(webjobName) : CliMain(args);
            }
            finally
            {
                TelemetryHelper.Client.Flush();
                if (Debugger.IsAttached)
                {
                    Console.WriteLine("Press [Enter] to continue");
                    Console.ReadLine();
                }
            }
        }

        private static int WebJobMain(string webjobName)
        {
            var startTicks = Environment.TickCount;
            Events.WebJobRenewalStarted(webjobName);

            Trace.TraceInformation("Web App SSL renewal job ({0}) started", webjobName);

            var renewer = new AppSettingsRenewer(
                new RenewalManager(),
                new AppSettingsRenewalParamsReader(new AppSettingsReader(ConfigurationManager.AppSettings, ConfigurationManager.ConnectionStrings)),
                new SendGridNotifier(ConfigurationManager.ConnectionStrings[Constants.KeyPrefix + Constants.SendGridApiKey]?.ConnectionString));
            try
            {
                renewer.Renew().GetAwaiter().GetResult();
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
            catch (ArgumentParsingException e)
            {
                Trace.TraceError(e.Message);
                Console.WriteLine(e.HelpText);
                TelemetryHelper.Client.TrackException(e);

                return ExitCodes.ArgumentError;
            }
            catch (ArgumentValidationException e)
            {
                Trace.TraceError(e.Message);
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