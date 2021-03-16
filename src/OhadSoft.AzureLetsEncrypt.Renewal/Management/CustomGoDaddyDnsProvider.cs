using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LetsEncrypt.Azure.Core.V2.DnsProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    /// <summary>
    /// Implementation copied from https://github.com/sjkp/letsencrypt-azure/blob/master/src/LetsEncrypt.Azure.Core.V2/DnsProviders/GoDaddyDnsProvider.cs and added
    /// implementation of cleanup. Without the cleanup subsequent requests are not always validated by Let's Ecnrypt.
    /// </summary>
    /// <seealso cref="LetsEncrypt.Azure.Core.V2.DnsProviders.IDnsProvider" />
    internal sealed class CustomGoDaddyDnsProvider : IDnsProvider, IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        private bool isDisposed = false;

        public CustomGoDaddyDnsProvider(GoDaddyDnsSettings settings)
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri($"https://api.godaddy.com/v1/domains/{settings.Domain}/");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"sso-key {settings.ApiKey}:{settings.ApiSecret}");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Shopper-Id", settings.ShopperId);
        }

        public int MinimumTtl => 600;

        public async Task Cleanup(string recordSetName)
        {
            var res = await httpClient.GetAsync(new Uri($"records/TXT", UriKind.Relative)).ConfigureAwait(false);
            res.EnsureSuccessStatusCode();

            var txtRecords = JArray.Parse(await res.Content.ReadAsStringAsync().ConfigureAwait(false));

            // Because LetsEncrypt.Azure.Core.V2 call cleanup with the value of the record instead of the record name
            // we check for both of them to work if they fix it in the future.
            var acmeTxtRecord = txtRecords.FirstOrDefault(t => t["name"].ToString() == recordSetName || t["data"].ToString() == recordSetName);
            if (acmeTxtRecord != null)
            {
                txtRecords.Remove(acmeTxtRecord);

                // If there are no more TXT records, GoDaddy will not accept to remove existing entries with an
                // empty array. Instead add a dummy record.
                if (!txtRecords.Any())
                {
                    txtRecords.Add(new JObject(new JProperty("name", "_acme-dummy"), new JProperty("ttl", 600), new JProperty("type", "TXT"), new JProperty("data", "_")));
                }

                using (var content = new StringContent(JsonConvert.SerializeObject(txtRecords, jsonSerializerSettings), Encoding.UTF8, "application/json"))
                {
                    res = await httpClient.PutAsync(new Uri($"records/TXT", UriKind.Relative), content).ConfigureAwait(false);
                    res.EnsureSuccessStatusCode();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task PersistChallenge(string recordSetName, string recordValue)
        {
            var acmeChallengeRecord = new[]
            {
                new DnsRecord
                {
                    Data = recordValue,
                    Name = recordSetName,
                    Ttl = MinimumTtl,
                    Type = "TXT",
                },
            };

            using (var content = new StringContent(JsonConvert.SerializeObject(acmeChallengeRecord, jsonSerializerSettings), Encoding.UTF8, "application/json"))
            {
                var res = await httpClient.PutAsync(new Uri($"records/TXT/{recordSetName}", UriKind.Relative), content).ConfigureAwait(false);
                res.EnsureSuccessStatusCode();
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || isDisposed)
            {
                return;
            }

            isDisposed = true;
            httpClient.Dispose();
        }

        public class GoDaddyDnsSettings
        {
            public string ApiKey { get; set; }
            public string ApiSecret { get; set; }
            public string ShopperId { get; set; }
            public string Domain { get; set; }
        }

        public class DnsRecord
        {
            public string Data { get; set; }
            public string Name { get; set; }
            public int Ttl { get; set; }
            public string Type { get; set; }
        }
    }
}
