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

            ParserResult<Options> parserResult;
            using (var parser = new Parser())
            {
                parserResult = parser.ParseArguments<Options>(args);
            }

            if (parserResult.Tag != ParserResultType.Parsed)
            {
                throw new ArgumentParsingException("Could not parse command-line arguments", GetUsage(parserResult));
            }

            var parsed = ((Parsed<Options>)parserResult).Value;

            RenewalParameters renewalParameters;
            try
            {
                renewalParameters = new RenewalParameters(
                    parsed.SubscriptionId,
                    parsed.TenantId,
                    parsed.ResourceGroup,
                    parsed.WebApp,
                    parsed.Hosts,
                    parsed.Email,
                    parsed.ClientId,
                    parsed.ClientSecret,
                    parsed.ServicePlanResourceGroup,
                    groupName: null,
                    parsed.SiteSlotName,
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
            catch (ArgumentException e)
            {
                throw new ArgumentValidationException(e);
            }

            return renewalParameters;
        }

        private static string GetUsage(ParserResult<Options> parserResult)
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