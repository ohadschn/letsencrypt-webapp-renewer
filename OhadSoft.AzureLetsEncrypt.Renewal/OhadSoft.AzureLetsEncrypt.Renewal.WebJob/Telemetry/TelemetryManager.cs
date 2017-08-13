using System;
using System.Diagnostics.CodeAnalysis;
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
    internal static class TelemetryManager
    {
        public static readonly TelemetryClient Client = new TelemetryClient();

        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Lifetime is managed by Application Insights")]
        public static void Setup(string instrumentationKey)
        {
            TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey ?? throw new ArgumentNullException(nameof(instrumentationKey));

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
    }
}