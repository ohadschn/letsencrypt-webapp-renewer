using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings
{
    internal class AppSettingsRenewalParamsReader : IAppSettingsRenewalParamsReader
    {
        public const string KeyPrefix = "letsencrypt:";

        private const string SubscriptionIdKey = "subscriptionId";
        private const string TenantIdKey = "tenantId";
        private const string ResourceGroupKey = "resourceGroup";
        private const string HostsKey = "hosts";
        private const string EmailKey = "email";
        private const string ClientIdKey = "clientId";
        private const string ClientSecretKey = "clientSecret";
        private const string ServicePlanResourceGroupKey = "servicePlanResourceGroup";
        private const string SiteSlotNameKey = "siteSlotName";
        private const string UseIpBasedSslKey = "useIpBasedSsl";
        private const string RsaKeyLengthKey = "rsaKeyLength";
        private const string AcmeBaseUriKey = "acmeBaseUri";

        private readonly IAppSettingsReader m_appSettings;

        public AppSettingsRenewalParamsReader(IAppSettingsReader appSettings)
        {
            m_appSettings = appSettings;
        }

        public IReadOnlyCollection<RenewalParameters> Read()
        {
            Trace.TraceInformation("Parsing Web Apps for SSL renewal from webjob/site configuration...");
            var webApps = m_appSettings.GetDelimitedList(KeyPrefix + "webApps");
            if (webApps.Any(String.IsNullOrWhiteSpace))
            {
                throw new ConfigurationErrorsException("webApp list contains a whitespace-only entry");
            }

            Trace.TraceInformation("Parsed web apps for SSL renewal: {0}", String.Join("; ", webApps));

            var adminParams = GetSharedParams();
            Trace.TraceInformation("Parsed common admin parameters: {0}", adminParams);

            var webAppRenewalInfos = webApps.Select(wa => GetWebAppRenewalInfo(wa, adminParams)).ToArray();

            Trace.TraceInformation("Completed parsing of Web App SSL cert renewal information");
            return webAppRenewalInfos;
        }

        private SharedRenewalParameters GetSharedParams()
        {
            return new SharedRenewalParameters(
                GetCommonSetting(ResourceGroupKey),
                GetCommonGuidSetting(SubscriptionIdKey),
                GetCommonSetting(TenantIdKey),
                GetCommonGuidSetting(ClientIdKey),
                GetCommonConnectionString(ClientSecretKey),
                GetCommonSetting(EmailKey),
                GetCommonSetting(ServicePlanResourceGroupKey),
                GetCommonBooleanSetting(UseIpBasedSslKey),
                GetCommonInt32Setting(RsaKeyLengthKey),
                GetCommonUriSetting(AcmeBaseUriKey));
        }

        private RenewalParameters GetWebAppRenewalInfo(string webApp, SharedRenewalParameters sharedRenewalParams)
        {
            Trace.TraceInformation("Parsing SSL renewal parameters for web app '{0}'...", webApp);

            try
            {
              // ReSharper disable PossibleInvalidOperationException
                return new RenewalParameters(
                    ResolveGuidSetting(SubscriptionIdKey, webApp, sharedRenewalParams.SubscriptionId),
                    ResolveSetting(TenantIdKey, webApp, sharedRenewalParams.TenantId),
                    ResolveSetting(ResourceGroupKey, webApp, sharedRenewalParams.ResourceGroup),
                    webApp,
                    m_appSettings.GetDelimitedList(BuildConfigKey(HostsKey, webApp)),
                    ResolveSetting(EmailKey, webApp, sharedRenewalParams.Email),
                    ResolveGuidSetting(ClientIdKey, webApp, sharedRenewalParams.ClientId),
                    ResolveConnectionString(ClientSecretKey, webApp, sharedRenewalParams.ClientSecret),
                    m_appSettings.GetStringOrDefault(BuildConfigKey(ServicePlanResourceGroupKey, webApp), sharedRenewalParams.ServicePlanResourceGroup),
                    m_appSettings.GetStringOrDefault(BuildConfigKey(SiteSlotNameKey, webApp)),
                    m_appSettings.GetBooleanOrDefault(BuildConfigKey(UseIpBasedSslKey, webApp), sharedRenewalParams.UseIpBasedSsl ?? false).Value,
                    m_appSettings.GetInt32OrDefault(BuildConfigKey(RsaKeyLengthKey, webApp), sharedRenewalParams.RsaKeyLength ?? 2048).Value,
                    m_appSettings.GetUriOrDefault(BuildConfigKey(AcmeBaseUriKey, webApp), UriKind.Absolute, sharedRenewalParams.AcmeBaseUri));
            } // ReSharper restore PossibleInvalidOperationException
            catch (ArgumentException e)
            {
                throw new ConfigurationErrorsException("Error parsing SSL renewal parameters for web app: " + webApp, e);
            }
        }

        private string GetCommonSetting(string key)
        {
            return m_appSettings.GetStringOrDefault(BuildConfigKey(key));
        }

        private Guid? GetCommonGuidSetting(string key)
        {
            return m_appSettings.GetGuidOrDefault(BuildConfigKey(key));
        }

        private bool? GetCommonBooleanSetting(string key)
        {
            return m_appSettings.GetBooleanOrDefault(BuildConfigKey(key));
        }

        private int? GetCommonInt32Setting(string key)
        {
            return m_appSettings.GetInt32OrDefault(key);
        }

        private Uri GetCommonUriSetting(string key)
        {
            return m_appSettings.GetUriOrDefault(key);
        }

        private string GetCommonConnectionString(string key)
        {
            return m_appSettings.GetConnectionStringOrDefault(BuildConfigKey(key));
        }

        private string ResolveSetting(string key, string webApp, string commonSetting)
        {
            var configKey = BuildConfigKey(key, webApp);

            return commonSetting == null
                ? m_appSettings.GetString(configKey)
                : m_appSettings.GetStringOrDefault(configKey, commonSetting);
        }

        private Guid ResolveGuidSetting(string key, string webApp, Guid? commonSetting)
        {
            var configKey = BuildConfigKey(key, webApp);

            // ReSharper disable once PossibleInvalidOperationException
            return commonSetting == null
                ? m_appSettings.GetGuid(configKey)
                : m_appSettings.GetGuidOrDefault(configKey, commonSetting).Value;
        }

        private string ResolveConnectionString(string key, string webApp, string commonConnectionString)
        {
            var configKey = BuildConfigKey(key, webApp);

            return commonConnectionString == null
                ? m_appSettings.GetConnectionString(configKey)
                : m_appSettings.GetConnectionStringOrDefault(configKey, commonConnectionString);
        }

        private static string BuildConfigKey(string key, string webApp = null)
        {
            var builder = new StringBuilder(KeyPrefix);
            if (webApp != null)
            {
                builder.Append(webApp + "-");
            }

            builder.Append(key);
            return builder.ToString();
        }
    }
}