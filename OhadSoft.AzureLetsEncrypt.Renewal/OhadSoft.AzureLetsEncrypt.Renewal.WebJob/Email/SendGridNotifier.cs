using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Email
{
    internal class SendGridNotifier : IEmailNotifier
    {
        private readonly string m_apiKey;

        public SendGridNotifier(string apiKey)
        {
            if (apiKey != null && apiKey.All(Char.IsWhiteSpace))
            {
                throw new ArgumentException("SendGrid API key cannot be whitespace", nameof(apiKey));
            }

            m_apiKey = apiKey;
        }

        public async Task NotifyAsync(RenewalParameters renewalParams)
        {
            if (renewalParams == null)
            {
                throw new ArgumentNullException(nameof(renewalParams));
            }

            if (m_apiKey == null)
            {
                Trace.TraceWarning(
                    "E-mail notification for web app {0} skipped because the '{1}' connection string was not set",
                    renewalParams.WebApp,
                    Constants.SendGridApiKey);
                return;
            }

            var message = MailHelper.CreateSingleEmail(
                new EmailAddress("letsencrypt-webapp-renewer@ohadsoft.com", "Azure Web App Let's Encrypt Renewer"),
                new EmailAddress(renewalParams.Email),
                "SSL Certificate renewal complete for web app: " + renewalParams.WebApp,
                "Renewal parameters:" + Environment.NewLine + renewalParams,
                null);

            Trace.TraceInformation("Sending e-mail notification for {0}... ", renewalParams.WebApp);
            await new SendGridClient(m_apiKey).SendEmailAsync(message);
            Trace.TraceInformation("Finished sending e-mail notification for: {0}", renewalParams.WebApp);
        }
    }
}