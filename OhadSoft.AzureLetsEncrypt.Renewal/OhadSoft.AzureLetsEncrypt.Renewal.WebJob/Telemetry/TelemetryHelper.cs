using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.WindowsServer;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Telemetry
{
    internal static class TelemetryHelper
    {
        public static readonly TelemetryClient Client = new TelemetryClient();

        public static bool TelemetryInitialized => Client.InstrumentationKey != null;

        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Lifetime is managed by Application Insights")]
        public static void Setup()
        {
            TelemetryConfiguration.Active.InstrumentationKey = "32cf968e-40d4-42d3-a2de-037140fd4371";

            AddAndInitializeModule<UnhandledExceptionTelemetryModule>();
            AddAndInitializeModule<UnobservedExceptionTelemetryModule>();

            // for performance counter collection see: http://apmtips.com/blog/2015/10/07/performance-counters-in-non-web-applications/
            AddAndInitializeModule<PerformanceCollectorModule>();

            // for more information on QuickPulse see: http://apmtips.com/blog/2017/02/13/enable-application-insights-live-metrics-from-code/ (note the corresponding processor below)
            var quickPulseModule = AddAndInitializeModule<QuickPulseTelemetryModule>();

#pragma warning disable S1313
            AddAndInitializeModule(() => new DependencyTrackingTelemetryModule
            {
                ExcludeComponentCorrelationHttpHeadersOnDomains =
                {
                    "core.windows.net",
                    "core.chinacloudapi.cn",
                    "core.cloudapi.de",
                    "core.usgovcloudapi.net",
                    "localhost",
                    "127.0.0.1"
                }
            });
#pragma warning restore S1313

            TelemetryConfiguration.Active.TelemetryInitializers.Add(new BuildInfoConfigComponentVersionTelemetryInitializer());
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new DeviceTelemetryInitializer());
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new AzureWebAppRoleEnvironmentTelemetryInitializer());

            TelemetryConfiguration.Active.TelemetryProcessorChainBuilder
                .Use(next =>
                {
                    var processor = new QuickPulseTelemetryProcessor(next);
                    quickPulseModule.RegisterTelemetryProcessor(processor);
                    return processor;
                })
                .Use(next => new AutocollectedMetricsExtractor(next))
                .Build();

            // ReSharper disable once UseObjectOrCollectionInitializer
            TelemetryConfiguration.Active.TelemetryChannel = new InMemoryChannel();
#if DEBUG
            TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = true;
#endif

            AppDomain.CurrentDomain.ProcessExit += (s, e) => Client.Flush();
        }

        private static T AddAndInitializeModule<T>(Func<T> factory = null)
            where T : ITelemetryModule, new()
        {
            var module = (factory != null) ? factory() : new T();
            module.Initialize(TelemetryConfiguration.Active);
            TelemetryModules.Instance.Modules.Add(module);
            return module;
        }

        private static readonly byte[] Pepper = Guid.Parse("3a7d41f4-e0cd-4a00-a42a-61de1700bc6b").ToByteArray();
        public static string Hash(string str)
        {
            if (str == null)
            {
                TrackInternalError(new ArgumentNullException(nameof(str)));
                return string.Empty;
            }

            // https://security.stackexchange.com/questions/17994/with-pbkdf2-what-is-an-optimal-hash-size-in-bytes-what-about-the-size-of-the-s
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(str, Pepper, 10000))
            {
                return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(20));
            }
        }

        public static void TrackInternalError(Exception exception)
        {
            exception = exception ?? new InternalErrorException("internal error tracked with null exception", null);
            try
            {
                // we're throwing to get a full exception with stack trace in the catch clause below
                throw new InternalErrorException(exception.Message, exception);
            }
            catch (InternalErrorException e)
            {
                Client.TrackException(e);
            }
        }
    }
}