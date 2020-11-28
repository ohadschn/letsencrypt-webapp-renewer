using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            Trace.TraceInformation("Parsing Web Apps for SSL renewal from WebJob/site configuration...");
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
            AzureEnvironmentParams sharedWebAppEnvironment;
            try
            {
                sharedWebAppEnvironment = new AzureEnvironmentParams(
                    GetCommonSetting(Constants.TenantIdKey),
                    GetCommonGuidSetting(Constants.SubscriptionIdKey),
                    GetCommonGuidSetting(Constants.ClientIdKey),
                    GetCommonConnectionString(Constants.ClientSecretKey),
                    GetCommonSetting(Constants.ResourceGroupKey),
                    true);
            }
            catch (ArgumentException e)
            {
                throw new ConfigurationErrorsException("Error parsing shared Web App environment parameters", e);
            }

            AzureEnvironmentParams sharedAzureDnsEnvironment;
            try
            {
                sharedAzureDnsEnvironment = new AzureEnvironmentParams(
                    GetCommonSetting(Constants.AzureDnsTenantIdKey),
                    GetCommonGuidSetting(Constants.AzureDnsSubscriptionIdKey),
                    GetCommonGuidSetting(Constants.AzureDnsClientIdKey),
                    GetCommonConnectionString(Constants.AzureDnsClientSecretKey),
                    GetCommonSetting(Constants.AzureDnsResourceGroupKey),
                    true);
            }
            catch (ArgumentException e)
            {
                throw new ConfigurationErrorsException("Error parsing shared Azure DNS environment parameters", e);
            }

            try
            {
                return new SharedRenewalParameters(
                    sharedWebAppEnvironment,
                    GetCommonSetting(Constants.EmailKey),
                    GetCommonSetting(Constants.FromEmailKey),
                    GetCommonSetting(Constants.ServicePlanResourceGroupKey),
                    sharedAzureDnsEnvironment,
                    GetCommonSetting(Constants.AzureDnsZoneNameKey),
                    GetCommonSetting(Constants.AzureDnsRelativeRecordSetNameKey),
                    GetCommonBooleanSetting(Constants.UseIpBasedSslKey),
                    GetCommonInt32Setting(Constants.RsaKeyLengthKey),
                    GetCommonUriSetting(Constants.AcmeBaseUriKey),
                    GetCommonSetting(Constants.WebRootPathKey),
                    GetCommonInt32Setting(Constants.RenewXNumberOfDaysBeforeExpirationKey),
                    GetCommonUriSetting(Constants.AzureAuthenticationEndpointKey),
                    GetCommonUriSetting(Constants.AzureTokenAudienceKey),
                    GetCommonUriSetting(Constants.AzureManagementEndpointKey),
                    GetCommonSetting(Constants.AzureDefaultWebSiteDomainNameKey));
            }
            catch (ArgumentException e)
            {
                throw new ConfigurationErrorsException("Error parsing shared renewal parameters", e);
            }
        }

        private RenewalParameters GetWebAppRenewalInfo(string webApp, SharedRenewalParameters sharedRenewalParams)
        {
            Trace.TraceInformation("Parsing SSL renewal parameters for web app '{0}'...", webApp);
            ParseWebAppToken(webApp, out var webAppName, out var siteSlotName, out var groupName);

            AzureEnvironmentParams webAppEnvironment;
            try
            {
                webAppEnvironment = new AzureEnvironmentParams(
                    ResolveSetting(Constants.TenantIdKey, webApp, sharedRenewalParams.WebAppEnvironment.TenantId),
                    ResolveGuidSetting(Constants.SubscriptionIdKey, webApp, sharedRenewalParams.WebAppEnvironment.SubscriptionId),
                    ResolveGuidSetting(Constants.ClientIdKey, webApp, sharedRenewalParams.WebAppEnvironment.ClientId),
                    ResolveConnectionString(Constants.ClientSecretKey, webApp, sharedRenewalParams.WebAppEnvironment.ClientSecret),
                    ResolveSetting(Constants.ResourceGroupKey, webApp, sharedRenewalParams.WebAppEnvironment.ResourceGroup));
            }
            catch (ArgumentException e)
            {
                throw new ConfigurationErrorsException("Error parsing Web App environment parameters for web app: " + webApp, e);
            }

            AzureEnvironmentParams azureDnsEnvironment;
            try
            {
                azureDnsEnvironment = new AzureEnvironmentParams(
                    ResolveOptionalSetting(Constants.AzureDnsTenantIdKey, webApp, sharedRenewalParams.AzureDnsEnvironment.TenantId ?? webAppEnvironment.TenantId),
                    ResolveGuidSetting(Constants.AzureDnsSubscriptionIdKey, webApp, sharedRenewalParams.AzureDnsEnvironment.SubscriptionId ?? webAppEnvironment.SubscriptionId),
                    ResolveGuidSetting(Constants.AzureDnsClientIdKey, webApp, sharedRenewalParams.AzureDnsEnvironment.ClientId ?? webAppEnvironment.ClientId),
                    ResolveConnectionString(Constants.AzureDnsClientSecretKey, webApp, sharedRenewalParams.AzureDnsEnvironment.ClientSecret ?? webAppEnvironment.ClientSecret),
                    ResolveSetting(Constants.AzureDnsResourceGroupKey, webApp, sharedRenewalParams.AzureDnsEnvironment.ResourceGroup ?? webAppEnvironment.ResourceGroup));
            }
            catch (ArgumentException e)
            {
                throw new ConfigurationErrorsException("Error parsing Azure DNS environment parameters for web app: " + webApp, e);
            }

            try
            {
                return new RenewalParameters(
                    webAppEnvironment,
                    webAppName,
                    m_appSettings.GetDelimitedList(BuildConfigKey(Constants.HostsKey, webApp)),
                    ResolveSetting(Constants.EmailKey, webApp, sharedRenewalParams.Email),
                    ResolveSetting(Constants.FromEmailKey, webApp, sharedRenewalParams.FromEmail),
                    ResolveOptionalSetting(Constants.ServicePlanResourceGroupKey, webApp, sharedRenewalParams.ServicePlanResourceGroup),
                    groupName,
                    siteSlotName,
                    azureDnsEnvironment,
                    ResolveOptionalSetting(Constants.AzureDnsZoneNameKey, webApp, sharedRenewalParams.AzureDnsZoneName),
                    ResolveOptionalSetting(Constants.AzureDnsRelativeRecordSetNameKey, webApp, sharedRenewalParams.AzureDnsRelativeRecordSetName),
                    ResolveOptionalBooleanSetting(Constants.UseIpBasedSslKey, webApp, sharedRenewalParams.UseIpBasedSsl, false),
                    ResolveOptionalInt32Setting(Constants.RsaKeyLengthKey, webApp, sharedRenewalParams.RsaKeyLength, 2048),
                    ResolveOptionalUriSetting(Constants.AcmeBaseUriKey, webApp, sharedRenewalParams.AcmeBaseUri),
                    ResolveOptionalSetting(Constants.WebRootPathKey, webApp, sharedRenewalParams.WebRootPath),
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

        private static void ParseWebAppToken(string webApp, out string webAppName, out string siteSlotName, out string groupName)
        {
            webAppName = webApp;
            siteSlotName = null;
            groupName = null;

            var match = Regex.Match(webAppName, @"^(.*)\[(.*)\]$");
            if (match.Success)
            {
                webAppName = match.Groups[1].Value;
                groupName = match.Groups[2].Value;
            }

            match = Regex.Match(webAppName, "^(.*){(.*)}$");
            if (match.Success)
            {
                webAppName = match.Groups[1].Value;
                siteSlotName = match.Groups[2].Value;
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