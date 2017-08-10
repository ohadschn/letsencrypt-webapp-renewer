using System;
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
            if (apiKey != null && apiKey.All(char.IsWhiteSpace))
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
                Console.WriteLine("E-mail notification for web app {0} skipped because SendGrid API key is not configured", renewalParams.WebApp);
                return;
            }

            var message = MailHelper.CreateSingleEmail(
                new EmailAddress("letsencrypt-webapp-renewer@ohadsoft.com", "Azure Web App Let's Encrypt Renewer"),
                new EmailAddress(renewalParams.Email),
                "SSL Certificate renewal complete for web app: " + renewalParams.WebApp,
                "Renewal parameters:" + Environment.NewLine + renewalParams,
                null);

            Console.WriteLine("Sending e-mail notification for {0}... ", renewalParams.WebApp);
            await new SendGridClient(m_apiKey).SendEmailAsync(message);
            Console.WriteLine("Finished sending e-mail notification for: {0}", renewalParams.WebApp);
        }
    }
}