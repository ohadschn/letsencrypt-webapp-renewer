using System;
using System.Collections.Generic;
using OhadSoft.AzureLetsEncrypt.Renewal.Management;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.WebJob
{
    public sealed class RenewalParametersComparer : Comparer<RenewalParameters>
    {
        public override int Compare(RenewalParameters x, RenewalParameters y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (ReferenceEquals(null, y))
            {
                return 1;
            }

            if (ReferenceEquals(null, x))
            {
                return -1;
            }

            var subscriptionIdComparison = x.SubscriptionId.CompareTo(y.SubscriptionId);
            if (subscriptionIdComparison != 0)
            {
                return subscriptionIdComparison;
            }

            var tenantIdComparison = string.Compare(x.TenantId, y.TenantId, StringComparison.Ordinal);
            if (tenantIdComparison != 0)
            {
                return tenantIdComparison;
            }

            var resourceGroupComparison = string.Compare(x.ResourceGroup, y.ResourceGroup, StringComparison.Ordinal);
            if (resourceGroupComparison != 0)
            {
                return resourceGroupComparison;
            }

            var webAppComparison = string.Compare(x.WebApp, y.WebApp, StringComparison.Ordinal);
            if (webAppComparison != 0)
            {
                return webAppComparison;
            }

            var emailComparison = string.Compare(x.Email, y.Email, StringComparison.Ordinal);
            if (emailComparison != 0)
            {
                return emailComparison;
            }

            var clientIdComparison = x.ClientId.CompareTo(y.ClientId);
            if (clientIdComparison != 0)
            {
                return clientIdComparison;
            }

            var clientSecretComparison = string.Compare(x.ClientSecret, y.ClientSecret, StringComparison.Ordinal);
            if (clientSecretComparison != 0)
            {
                return clientSecretComparison;
            }

            var useIpBasedSslComparison = x.UseIpBasedSsl.CompareTo(y.UseIpBasedSsl);
            if (useIpBasedSslComparison != 0)
            {
                return useIpBasedSslComparison;
            }

            return x.RsaKeyLength.CompareTo(y.RsaKeyLength);
        }
    }
}