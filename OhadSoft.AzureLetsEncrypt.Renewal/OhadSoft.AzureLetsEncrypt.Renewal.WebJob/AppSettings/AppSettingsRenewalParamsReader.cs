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
        private readonly IAppSettingsReader m_appSettings;

        public AppSettingsRenewalParamsReader(IAppSettingsReader appSettings)
        {
            m_appSettings = appSettings;
        }

        public IReadOnlyCollection<RenewalParameters> Read()
        {
            Trace.TraceInformation("Parsing Web Apps for SSL renewal from webjob/site configuration...");
            var webApps = m_appSettings.GetDelimitedList(Constants.KeyPrefix + "webApps");
            if (webApps.Any(String.IsNullOrWhiteSpace))
            {
                throw new ConfigurationErrorsException("webApp list contains a whitespace-only entry");
            }

            Trace.TraceInformation("Parsed web apps for SSL renewal: {0}", String.Join("; ", webApps));

            var sharedParams = GetSharedParams();
            Trace.TraceInformation("Parsed shared parameters: {0}", sharedParams);

            var webAppRenewalInfos = webApps.Select(wa => GetWebAppRenewalInfo(wa, sharedParams)).ToArray();

            Trace.TraceInformation("Completed parsing of Web App SSL cert renewal information");
            return webAppRenewalInfos;
        }

        private SharedRenewalParameters GetSharedParams()
        {
            return new SharedRenewalParameters(
                GetCommonSetting(Constants.ResourceGroupKey),
                GetCommonGuidSetting(Constants.SubscriptionIdKey),
                GetCommonSetting(Constants.TenantIdKey),
                GetCommonGuidSetting(Constants.ClientIdKey),
                GetCommonConnectionString(Constants.ClientSecretKey),
                GetCommonSetting(Constants.EmailKey),
                GetCommonSetting(Constants.ServicePlanResourceGroupKey),
                GetCommonBooleanSetting(Constants.UseIpBasedSslKey),
                GetCommonInt32Setting(Constants.RsaKeyLengthKey),
                GetCommonUriSetting(Constants.AcmeBaseUriKey),
                GetCommonInt32Setting(Constants.RenewXNumberOfDaysBeforeExpirationKey),
                GetCommonUriSetting(Constants.AzureAuthenticationEndpointKey),
                GetCommonUriSetting(Constants.AzureTokenAudienceKey),
                GetCommonUriSetting(Constants.AzureManagementEndpointKey),
                GetCommonSetting(Constants.AzureDefaultWebSiteDomainNameKey));
        }

        private RenewalParameters GetWebAppRenewalInfo(string webApp, SharedRenewalParameters sharedRenewalParams)
        {
            Trace.TraceInformation("Parsing SSL renewal parameters for web app '{0}'...", webApp);

            try
            {
                return new RenewalParameters(
                    ResolveGuidSetting(Constants.SubscriptionIdKey, webApp, sharedRenewalParams.SubscriptionId),
                    ResolveSetting(Constants.TenantIdKey, webApp, sharedRenewalParams.TenantId),
                    ResolveSetting(Constants.ResourceGroupKey, webApp, sharedRenewalParams.ResourceGroup),
                    webApp,
                    m_appSettings.GetDelimitedList(BuildConfigKey(Constants.HostsKey, webApp)),
                    ResolveSetting(Constants.EmailKey, webApp, sharedRenewalParams.Email),
                    ResolveGuidSetting(Constants.ClientIdKey, webApp, sharedRenewalParams.ClientId),
                    ResolveConnectionString(Constants.ClientSecretKey, webApp, sharedRenewalParams.ClientSecret),
                    ResolveOptionalSetting(Constants.ServicePlanResourceGroupKey, webApp, sharedRenewalParams.ServicePlanResourceGroup),
                    ResolveOptionalSetting(Constants.SiteSlotNameKey, webApp),
                    ResolveOptionalBooleanSetting(Constants.UseIpBasedSslKey, webApp, sharedRenewalParams.UseIpBasedSsl, false),
                    ResolveOptionalInt32Setting(Constants.RsaKeyLengthKey, webApp, sharedRenewalParams.RsaKeyLength, 2048),
                    ResolveOptionalUriSetting(Constants.AcmeBaseUriKey, webApp, sharedRenewalParams.AcmeBaseUri),
                    ResolveOptionalInt32Setting(Constants.RenewXNumberOfDaysBeforeExpirationKey, webApp, sharedRenewalParams.RenewXNumberOfDaysBeforeExpiration, -1),
                    ResolveOptionalUriSetting(Constants.AzureAuthenticationEndpointKey, webApp, sharedRenewalParams.AuthenticationUri),
                    ResolveOptionalUriSetting(Constants.AzureTokenAudienceKey, webApp, sharedRenewalParams.AzureTokenAudience),
                    ResolveOptionalUriSetting(Constants.AzureManagementEndpointKey, webApp, sharedRenewalParams.AzureManagementEndpoint),
                    ResolveOptionalSetting(Constants.AzureDefaultWebSiteDomainNameKey, webApp, sharedRenewalParams.AzureDefaultWebsiteDomainName));
            }
            catch (ArgumentException e)
            {
                throw new ConfigurationErrorsException("Error parsing SSL renewal parameters for web app: " + webApp, e);
            }
        }

        private string ResolveOptionalSetting(string key, string webApp, string defaultValue = null)
        {
            return m_appSettings.GetStringOrDefault(BuildConfigKey(key, webApp), defaultValue);
        }

        private Uri ResolveOptionalUriSetting(string key, string webApp, Uri sharedValue, Uri defaultValue = null)
        {
            return m_appSettings.GetUriOrDefault(BuildConfigKey(key, webApp), UriKind.Absolute, sharedValue ?? defaultValue);
        }

        // ReSharper disable PossibleInvalidOperationException
        private bool ResolveOptionalBooleanSetting(string key, string webApp, bool? sharedValue, bool defaultValue)
        {
            return m_appSettings.GetBooleanOrDefault(BuildConfigKey(key, webApp), sharedValue ?? defaultValue).Value;
        }

        private int ResolveOptionalInt32Setting(string key, string webApp, int? sharedValue, int defaultValue)
        {
            return m_appSettings.GetInt32OrDefault(BuildConfigKey(key, webApp), sharedValue ?? defaultValue).Value;
        } // ReSharper restore PossibleInvalidOperationException

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
            return m_appSettings.GetInt32OrDefault(BuildConfigKey(key));
        }

        private Uri GetCommonUriSetting(string key)
        {
            return m_appSettings.GetUriOrDefault(BuildConfigKey(key));
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
            var builder = new StringBuilder(Constants.KeyPrefix);
            if (webApp != null)
            {
                builder.Append(webApp + "-");
            }

            builder.Append(key);
            return builder.ToString();
        }
    }
}