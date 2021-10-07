using System;
using CommandLine;
using CommandLine.Text;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Exceptions;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Cli
{
    internal class CommandlineRenewalParamsReader : ICommandlineRenewalParamsReader
    {
        public RenewalParameters Read(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            ParserResult<CliOptions> parserResult;
            using (var parser = new Parser())
            {
                parserResult = parser.ParseArguments<CliOptions>(args);
            }

            if (parserResult.Tag != ParserResultType.Parsed)
            {
                throw new ArgumentParsingException("Could not parse command-line arguments", GetUsage(parserResult));
            }

            var parsed = ((Parsed<CliOptions>)parserResult).Value;

            AzureEnvironmentParams webAppEnvironmentParams;
            try
            {
                webAppEnvironmentParams = GetWebAppEnvironmentParams(parsed);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentValidationException("Error parsing Web App environment parameters", e);
            }

            AzureEnvironmentParams azureDnsEnvironmentParams;
            try
            {
                azureDnsEnvironmentParams = GetAzureDnsEnvironmentParams(parsed);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentValidationException("Error parsing Azure DNS environment parameters", e);
            }

            GoDaddyEnvironmentParams goDaddyDnsEnvironmentParams;
            try
            {
                goDaddyDnsEnvironmentParams = GetGoDaddyDnsEnvironmentParams(parsed);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentValidationException("Error parsing GoDaddy DNS environment parameters", e);
            }

            RenewalParameters renewalParameters;
            try
            {
                renewalParameters = GetRenewalParameters(parsed, webAppEnvironmentParams, azureDnsEnvironmentParams, goDaddyDnsEnvironmentParams);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentValidationException("Error parsing renewal parameters", e);
            }

            return renewalParameters;
        }

        private static AzureEnvironmentParams GetWebAppEnvironmentParams(CliOptions parsed)
        {
            return new AzureEnvironmentParams(
                parsed.TenantId,
                parsed.SubscriptionId,
                parsed.ClientId,
                parsed.ClientSecret,
                parsed.ResourceGroup);
        }

        private static AzureEnvironmentParams GetAzureDnsEnvironmentParams(CliOptions parsed)
        {
            return new AzureEnvironmentParams(
                parsed.AzureDnsTenantId ?? parsed.TenantId,
                parsed.AzureDnsSubscriptionId ?? parsed.SubscriptionId,
                parsed.AzureDnsClientId ?? parsed.ClientId,
                parsed.AzureDnsClientSecret ?? parsed.ClientSecret,
                parsed.AzureDnsResourceGroup ?? parsed.ResourceGroup);
        }

        private static GoDaddyEnvironmentParams GetGoDaddyDnsEnvironmentParams(CliOptions parsed)
        {
            if (string.IsNullOrEmpty(parsed.GoDaddyDnsApiKey) ||
                string.IsNullOrEmpty(parsed.GoDaddyDnsApiSecret) ||
                string.IsNullOrEmpty(parsed.GoDaddyDnsDomain) ||
                string.IsNullOrEmpty(parsed.GoDaddyDnsShopperId))
            {
                return null;
            }

            return new GoDaddyEnvironmentParams(
                parsed.GoDaddyDnsApiKey,
                parsed.GoDaddyDnsApiSecret,
                parsed.GoDaddyDnsDomain,
                parsed.GoDaddyDnsShopperId);
        }

        private static RenewalParameters GetRenewalParameters(CliOptions parsed, AzureEnvironmentParams webAppEnvironmentParams, AzureEnvironmentParams azureDnsEnvironmentParams, GoDaddyEnvironmentParams goDaddyEnvironmentParams)
        {
            return new RenewalParameters(
                webAppEnvironmentParams,
                parsed.WebApp,
                parsed.Hosts,
                parsed.Email,
                parsed.FromEmail,
                parsed.ServicePlanResourceGroup,
                null,
                parsed.SiteSlotName,
                azureDnsEnvironmentParams,
                parsed.AzureDnsZoneName,
                parsed.AzureDnsRelativeRecordSetName,
                goDaddyEnvironmentParams,
                parsed.UseIpBasedSsl,
                parsed.RsaKeyLength,
                parsed.AcmeBaseUri,
                parsed.WebRootPath,
                parsed.RenewXNumberOfDaysBeforeExpiration,
                parsed.AzureAuthenticationEndpoint,
                parsed.AzureTokenAudience,
                parsed.AzureManagementEndpoint,
                parsed.AzureDefaultWebsiteDomainName);
        }

        private static string GetUsage(ParserResult<CliOptions> parserResult)
        {
            var autoBuild = HelpText.AutoBuild(parserResult);

            autoBuild.AddPostOptionsLine("Exit codes:");
            autoBuild.AddPostOptionsLine(Invariant($"{ExitCodes.Success} - Success"));
            autoBuild.AddPostOptionsLine(Invariant($"{ExitCodes.ArgumentError} - Bad argument(s)"));
            autoBuild.AddPostOptionsLine(Invariant($"{ExitCodes.UnexpectedException} - Unexpected error"));
            autoBuild.AddPostOptionsLine(String.Empty);
            autoBuild.AddPostOptionsLine("Consult the Let's Encrypt documentation for rate limits: https://letsencrypt.org/docs/rate-limits/");

            return autoBuild;
        }
    }
}