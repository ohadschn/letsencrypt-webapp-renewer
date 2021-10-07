using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LetsEncrypt.Azure.Core;
using LetsEncrypt.Azure.Core.Models;
using LetsEncrypt.Azure.Core.V2;
using LetsEncrypt.Azure.Core.V2.CertificateStores;
using LetsEncrypt.Azure.Core.V2.DnsProviders;
using LetsEncrypt.Azure.Core.V2.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.TraceSource;
using OhadSoft.AzureLetsEncrypt.Renewal.Configuration;
using OhadSoft.AzureLetsEncrypt.Renewal.Util;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public class RenewalManager : IRenewalManager
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        public const string DefaultWebsiteDomainName = "azurewebsites.net";
        public const string DefaultAcmeBaseUri = "https://acme-v02.api.letsencrypt.org/directory";
        public const string DefaultAuthenticationUri = "https://login.windows.net/";
        public const string DefaultAzureTokenAudienceService = "https://management.core.windows.net/";
        public const string DefaultManagementEndpoint = "https://management.azure.com";
#pragma warning restore S1075 // URIs should not be hardcoded

        private static readonly LoggerFactory s_loggerFactory = new LoggerFactory(new[]
        {
            new TraceSourceLoggerProvider(
                new SourceSwitch("letsencrypt-azure", "All"),
                new ConsoleTraceListener { Filter = new EventTypeFilter(SourceLevels.Information), TraceOutputOptions = TraceOptions.DateTime }),
        });

        public Task Renew(RenewalParameters renewalParams)
        {
            if (renewalParams == null)
            {
                throw new ArgumentNullException(nameof(renewalParams));
            }

            return RenewCore(renewalParams);
        }

        private static async Task RenewCore(RenewalParameters renewalParams)
        {
            Trace.TraceInformation("Adding / renewing SSL cert for '{0}' with parameters: {1}", GetWebAppFullName(renewalParams), renewalParams);

            var acmeConfig = GetAcmeConfig(renewalParams, CertificateHelper.GenerateSecurePassword());
            var webAppEnvironment = GetWebAppEnvironment(renewalParams);
            var certificateServiceSettings = new CertificateServiceSettings { UseIPBasedSSL = renewalParams.UseIpBasedSsl };
            var dnsProvider = GetDnsProvider(renewalParams);

            bool staging = acmeConfig.BaseUri.Contains("staging", StringComparison.OrdinalIgnoreCase);
            if (dnsProvider != null)
            {
                await GetDnsRenewalService(renewalParams, dnsProvider, webAppEnvironment).Run(
                    new AcmeDnsRequest
                    {
                        AcmeEnvironment = staging ? (AcmeEnvironment)new LetsEncryptStagingV2() : new LetsEncryptV2(),
                        RegistrationEmail = acmeConfig.RegistrationEmail,
                        Host = acmeConfig.Host,
                        PFXPassword = CertificateHelper.GenerateSecurePassword(),
                        CsrInfo = new CsrInfo(),
                    }, renewalParams.RenewXNumberOfDaysBeforeExpiration).ConfigureAwait(false);
            }
            else
            {
                var manager = CertificateManager.CreateKuduWebAppCertificateManager(webAppEnvironment, acmeConfig, certificateServiceSettings, new AuthProviderConfig());
                var addNewCert = await CheckCertAddition(renewalParams, webAppEnvironment, acmeConfig, staging).ConfigureAwait(false);
                if (addNewCert)
                {
                    await manager.AddCertificate().ConfigureAwait(false);
                }
            }

            Trace.TraceInformation("Let's Encrypt SSL certs & bindings renewed for '{0}'", renewalParams.WebApp);
        }

        private static async Task<bool> CheckCertAddition(
            RenewalParameters renewalParams,
            AzureWebAppEnvironment webAppEnvironment,
            AcmeConfig acmeConfig,
            bool staging)
        {
            if (renewalParams.RenewXNumberOfDaysBeforeExpiration <= 0)
            {
                return true;
            }

            var nonExpiringLetsEncryptHostNames = await CertificateHelper.GetNonExpiringLetsEncryptHostNames(webAppEnvironment, staging, renewalParams.RenewXNumberOfDaysBeforeExpiration).ConfigureAwait(false);
            Trace.TraceInformation("Let's Encrypt non-expiring host names (staging: {0}): {1}", staging, String.Join(", ", nonExpiringLetsEncryptHostNames));

            ICollection<string> missingHostNames = acmeConfig.Hostnames.Except(nonExpiringLetsEncryptHostNames, StringComparer.OrdinalIgnoreCase).ToArray();
            if (missingHostNames.Count > 0)
            {
                Trace.TraceInformation(
                    "Detected host name(s) with no associated / expiring Let's Encrypt certificates, will add a new certificate: {0}",
                    String.Join(", ", missingHostNames));
                return true;
            }

            Trace.TraceInformation("All host names associated with non-expiring Let's Encrypt certificates, no cert renewal necessary");
            return false;
        }

        private static LetsencryptService GetDnsRenewalService(RenewalParameters renewalParams, IDnsProvider dnsProvider, AzureWebAppEnvironment webAppEnvironment)
        {
            return new LetsencryptService(
                new AcmeClient(
                    dnsProvider,
                    new DnsLookupService(new Logger<DnsLookupService>(s_loggerFactory)),
                    new NullCertificateStore(),
                    new Logger<AcmeClient>(s_loggerFactory)),
                new NullCertificateStore(),
                new AzureWebAppService(
                    new[]
                    {
                        new AzureWebAppSettings(
                            webAppEnvironment.WebAppName,
                            webAppEnvironment.ResourceGroupName,
                            GetAzureServicePrincipal(webAppEnvironment),
                            GetAzureSubscription(webAppEnvironment),
                            webAppEnvironment.SiteSlotName,
                            webAppEnvironment.ServicePlanResourceGroupName,
                            renewalParams.UseIpBasedSsl),
                    },
                    new Logger<AzureWebAppService>(s_loggerFactory)),
                new Logger<LetsencryptService>(s_loggerFactory));
        }

        private static AzureSubscription GetAzureSubscription(IAzureEnvironment azureDnsEnvironment)
        {
            return new AzureSubscription
            {
                AzureRegion = GetAzureCloud(azureDnsEnvironment.TokenAudience?.ToString()),
                SubscriptionId = azureDnsEnvironment.SubscriptionId.ToString(),
                Tenant = azureDnsEnvironment.Tenant,
            };
        }

        private static AzureServicePrincipal GetAzureServicePrincipal(IAzureEnvironment azureDnsEnvironment)
        {
            return new AzureServicePrincipal
            {
                ClientId = azureDnsEnvironment.ClientId.ToString(),
                ClientSecret = azureDnsEnvironment.ClientSecret,
            };
        }

        private static string GetAzureCloud(string tokenAudience)
        {
            tokenAudience = tokenAudience ?? "https://management.core.windows.net/";

            if (tokenAudience.Contains(".de", StringComparison.OrdinalIgnoreCase))
            {
                return "AzureGermanCloud";
            }

            if (tokenAudience.Contains(".cn", StringComparison.OrdinalIgnoreCase))
            {
                return "AzureChinaCloud";
            }

            if (tokenAudience.Contains(".usgov", StringComparison.OrdinalIgnoreCase))
            {
                return "AzureUSGovernment";
            }

            if (tokenAudience.Contains("windows.net", StringComparison.OrdinalIgnoreCase))
            {
                return "AzureGlobalCloud";
            }

            throw new ConfigurationErrorsException($"Unknown token audience: {tokenAudience}");
        }

        private static IDnsProvider GetDnsProvider(RenewalParameters renewalParams)
        {
            var azureDnsEnvironment = GetAzureDnsEnvironment(renewalParams);
            if (azureDnsEnvironment != null)
            {
                return new AzureDnsProvider(
                           new AzureDnsSettings(
                               azureDnsEnvironment.ResourceGroupName,
                               azureDnsEnvironment.ZoneName,
                               GetAzureServicePrincipal(azureDnsEnvironment),
                               GetAzureSubscription(azureDnsEnvironment),
                               azureDnsEnvironment.RelativeRecordSetName));
            }

            if (!string.IsNullOrEmpty(renewalParams.GoDaddyDnsEnvironmentParams?.ApiKey) &&
                !string.IsNullOrEmpty(renewalParams.GoDaddyDnsEnvironmentParams?.ApiSecret) &&
                !string.IsNullOrEmpty(renewalParams.GoDaddyDnsEnvironmentParams?.Domain) &&
                !string.IsNullOrEmpty(renewalParams.GoDaddyDnsEnvironmentParams?.ShopperId))
            {
                return new CustomGoDaddyDnsProvider(
                           new CustomGoDaddyDnsProvider.GoDaddyDnsSettings()
                           {
                               ApiKey = renewalParams.GoDaddyDnsEnvironmentParams.ApiKey,
                               ApiSecret = renewalParams.GoDaddyDnsEnvironmentParams.ApiSecret,
                               Domain = renewalParams.GoDaddyDnsEnvironmentParams.Domain,
                               ShopperId = renewalParams.GoDaddyDnsEnvironmentParams.ShopperId,
                           });
            }

            Trace.TraceInformation($"Either {nameof(renewalParams.GoDaddyDnsEnvironmentParams.ApiKey)} or {nameof(renewalParams.GoDaddyDnsEnvironmentParams.ApiSecret)} or {nameof(renewalParams.GoDaddyDnsEnvironmentParams.Domain)} or {nameof(renewalParams.GoDaddyDnsEnvironmentParams.ShopperId)} are null for {GetWebAppFullName(renewalParams)}, will not use GoDaddy DNS challenge");
            return null;
        }

        private static IAzureDnsEnvironment GetAzureDnsEnvironment(RenewalParameters renewalParams)
        {
            var zoneName = renewalParams.AzureDnsZoneName;
            var relativeRecordSetName = renewalParams.AzureDnsRelativeRecordSetName;

            if (zoneName == null || relativeRecordSetName == null)
            {
                Trace.TraceInformation($"Either {nameof(zoneName)} or {nameof(relativeRecordSetName)} are null for {GetWebAppFullName(renewalParams)}, will not use Azure DNS challenge");
                return null;
            }

            Debug.Assert(renewalParams.AzureDnsEnvironmentParams?.SubscriptionId != null, "renewalParams.AzureDnsEnvironmentParams?.SubscriptionId != null");
            Debug.Assert(renewalParams.AzureDnsEnvironmentParams?.ClientId != null, "renewalParams.AzureDnsEnvironmentParams?.ClientId != null");

            return new AzureDnsEnvironment(
                renewalParams.AzureDnsEnvironmentParams.TenantId,
                renewalParams.AzureDnsEnvironmentParams.SubscriptionId.Value,
                renewalParams.AzureDnsEnvironmentParams.ClientId.Value,
                renewalParams.AzureDnsEnvironmentParams.ClientSecret,
                renewalParams.AzureDnsEnvironmentParams.ResourceGroup,
                zoneName,
                relativeRecordSetName);
        }

        private static AzureWebAppEnvironment GetWebAppEnvironment(RenewalParameters renewalParams)
        {
            Debug.Assert(renewalParams.WebAppEnvironmentParams?.SubscriptionId != null, "renewalParams.WebAppEnvironmentParams?.SubscriptionId != null");
            Debug.Assert(renewalParams.WebAppEnvironmentParams?.ClientId != null, "renewalParams.WebAppEnvironmentParams?.ClientId != null");

            return new AzureWebAppEnvironment(
                renewalParams.WebAppEnvironmentParams.TenantId,
                renewalParams.WebAppEnvironmentParams.SubscriptionId.Value,
                renewalParams.WebAppEnvironmentParams.ClientId.Value,
                renewalParams.WebAppEnvironmentParams.ClientSecret,
                renewalParams.WebAppEnvironmentParams.ResourceGroup,
                renewalParams.WebApp,
                renewalParams.ServicePlanResourceGroup,
                renewalParams.SiteSlotName)
            {
                WebRootPath = renewalParams.WebRootPath,
                AzureWebSitesDefaultDomainName = renewalParams.AzureDefaultWebsiteDomainName ?? DefaultWebsiteDomainName,
                AuthenticationEndpoint = renewalParams.AuthenticationUri ?? new Uri(DefaultAuthenticationUri),
                ManagementEndpoint = renewalParams.AzureManagementEndpoint ?? new Uri(DefaultManagementEndpoint),
                TokenAudience = renewalParams.AzureTokenAudience ?? new Uri(DefaultAzureTokenAudienceService),
            };
        }

        private static AcmeConfig GetAcmeConfig(RenewalParameters renewalParams, string pfxPassData)
        {
            Trace.TraceInformation("Generating secure PFX password for '{0}'...", renewalParams.WebApp);

            return new AcmeConfig
            {
                Host = renewalParams.Hosts[0],
                AlternateNames = renewalParams.Hosts.Skip(1).ToList(),
                RegistrationEmail = renewalParams.Email,
                RSAKeyLength = renewalParams.RsaKeyLength,
                PFXPassword = pfxPassData,
                BaseUri = (renewalParams.AcmeBaseUri ?? new Uri(DefaultAcmeBaseUri)).ToString(),
            };
        }

        private static string GetWebAppFullName(RenewalParameters renewalParams)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}{1}", renewalParams.WebApp, renewalParams.GroupName == null ? String.Empty : $"[{renewalParams.GroupName}]");
        }
    }
}