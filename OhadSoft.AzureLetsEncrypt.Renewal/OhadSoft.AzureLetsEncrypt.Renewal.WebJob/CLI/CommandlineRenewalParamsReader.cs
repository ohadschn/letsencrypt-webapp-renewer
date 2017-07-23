using System;
using System.Linq;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.CLI
{
    internal class CommandlineRenewalParamsReader : ICommandlineRenewalParamsReader
    {
        public RenewalParameters Read(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args.Any(String.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Null or whitespace parameters detected", nameof(args));
            }

            if (args.Length < 8 || args.Length > 11)
            {
                throw new ArgumentException("Invalid parameter count");
            }

            if (!Guid.TryParse(args[0], out Guid subscriptionId))
            {
                throw new ArgumentException("Could not parse subscription ID GUID");
            }

            var tenantId = args[1];
            var resourceGroup = args[2];
            var webApp = args[3];
            var hosts = args[4].Split(';').Select(s => s.Trim()).ToArray();
            var email = args[5];

            if (!Guid.TryParse(args[6], out Guid clientId))
            {
                throw new ArgumentException("Could not parse client ID GUID");
            }

            var clientSecret = args[7];

            bool useIpBasedSsl = false;
            if (args.Length >= 9 && !Boolean.TryParse(args[8], out useIpBasedSsl))
            {
                throw new ArgumentException("Could not parse useIpBasedSsl as boolean (true/false)");
            }

            int rsaKeyLength = 2048;
            if (args.Length >= 10 && !Int32.TryParse(args[9], out rsaKeyLength))
            {
                throw new ArgumentException("Could not parse RSA key length as 32-bit integer");
            }

            Uri acmeBasedUrl = null;
            if (args.Length >= 11 && !Uri.TryCreate(args[10], UriKind.Absolute, out acmeBasedUrl))
            {
                throw new ArgumentException("Could not parse ACME Base URL");
            }

            return new RenewalParameters(subscriptionId, tenantId, resourceGroup, webApp, hosts, email, clientId, clientSecret, useIpBasedSsl, rsaKeyLength, acmeBasedUrl);
        }
    }
}