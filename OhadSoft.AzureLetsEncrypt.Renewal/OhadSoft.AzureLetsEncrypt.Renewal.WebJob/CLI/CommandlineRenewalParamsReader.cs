using System;
using System.Linq;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

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

            if (args.Length < 8 || args.Length > 13)
            {
                throw new ArgumentException("Invalid parameter count");
            }

            if (!Guid.TryParse(args[0], out Guid subscriptionId))
            {
                throw new ArgumentException("Could not parse subscription ID GUID");
            }

            var tenantId = !String.IsNullOrWhiteSpace(args[1])
                ? args[1]
                : throw new ArgumentException("Tenant cannot be null or whitespace");

            var resourceGroup = !String.IsNullOrWhiteSpace(args[2])
                ? args[2]
                : throw new ArgumentException("Resource group cannot be null or whitespace");

            var webApp = !String.IsNullOrWhiteSpace(args[3])
                ? args[3]
                : throw new ArgumentException("Web app cannot be null or whitespace");

            var hosts = !String.IsNullOrWhiteSpace(args[4])
                ? args[4].Split(';').Select(s => s.Trim()).ToArray()
                : throw new ArgumentException("Hosts cannot be null or whitespace");

            var email = !String.IsNullOrWhiteSpace(args[5])
                ? args[5]
                : throw new ArgumentException("Email cannot be null or whitespace");

            if (!Guid.TryParse(args[6], out Guid clientId))
            {
                throw new ArgumentException("Could not parse client ID GUID");
            }

            var clientSecret = !String.IsNullOrWhiteSpace(args[7])
                ? args[7]
                : throw new ArgumentException("Client secret cannot be null or whitespace");

            string servicePlanResourceGroup = null;
            if (args.Length >= 9 && !String.IsNullOrWhiteSpace(args[8]))
            {
                servicePlanResourceGroup = args[8];
            }

            string siteSlotName = null;
            if (args.Length >= 10 && !String.IsNullOrWhiteSpace(args[9]))
            {
                siteSlotName = args[9];
            }

            bool useIpBasedSsl = false;
            if (args.Length >= 11 && !Boolean.TryParse(args[10], out useIpBasedSsl))
            {
                throw new ArgumentException("Could not parse useIpBasedSsl as boolean (true/false)");
            }

            int rsaKeyLength = 2048;
            if (args.Length >= 12 && !Int32.TryParse(args[11], out rsaKeyLength))
            {
                throw new ArgumentException("Could not parse RSA key length as 32-bit integer");
            }

            Uri acmeBaseUri = null;
            if (args.Length >= 13 && !Uri.TryCreate(args[12], UriKind.Absolute, out acmeBaseUri))
            {
                throw new ArgumentException("Could not parse ACME Base URI (as absolute)");
            }

            return new RenewalParameters(
                subscriptionId,
                tenantId,
                resourceGroup,
                webApp,
                hosts,
                email,
                clientId,
                clientSecret,
                servicePlanResourceGroup,
                siteSlotName,
                useIpBasedSsl,
                rsaKeyLength,
                acmeBaseUri);
        }
    }
}