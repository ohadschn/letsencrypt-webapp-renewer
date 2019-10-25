using System;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public sealed class AzureEnvironmentParams : IEquatable<AzureEnvironmentParams>
    {
        public bool AllowNull { get; }

        public string TenantId { get; }
        public Guid? SubscriptionId { get; }
        public string ResourceGroup { get; }
        public Guid? ClientId { get; }
        public string ClientSecret { get; }

        public AzureEnvironmentParams(string tenantId, Guid? subscriptionId, Guid? clientId, string clientSecret, string resourceGroup, bool allowNull = false)
        {
            AllowNull = allowNull;

            TenantId = ValidateParam(tenantId, nameof(tenantId), ParamValidator.VerifyString);
            SubscriptionId = ValidateParam(subscriptionId, nameof(subscriptionId), ParamValidator.VerifyGuid);
            ResourceGroup = ValidateParam(resourceGroup, nameof(resourceGroup), ParamValidator.VerifyString);
            ClientId = ValidateParam(clientId, nameof(clientId), ParamValidator.VerifyGuid);
            ClientSecret = ValidateParam(clientSecret, nameof(clientSecret), ParamValidator.VerifyString);
        }

        private T ValidateParam<T>(T value, string name, Func<T, string, T> validator)
            where T : class
        {
            if (value == null)
            {
                if (!AllowNull) throw new ArgumentNullException(name);
                return null;
            }

            return validator(value, name);
        }

        private T? ValidateParam<T>(T? nullable, string name, Func<T, string, T> validator)
            where T : struct
        {
            if (nullable == null)
            {
                if (!AllowNull) throw new ArgumentNullException(name);
                return null;
            }

            return validator((T)nullable, name);
        }

        public override string ToString()
        {
            return Invariant($"{nameof(TenantId)}: {TenantId}, {nameof(SubscriptionId)}: {SubscriptionId}, {nameof(ResourceGroup)}: {ResourceGroup}, {nameof(ClientId)}: {ClientId}");
        }

        public bool Equals(AzureEnvironmentParams other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(TenantId, other.TenantId)
                   && SubscriptionId.Equals(other.SubscriptionId)
                   && string.Equals(ResourceGroup, other.ResourceGroup)
                   && ClientId.Equals(other.ClientId)
                   && string.Equals(ClientSecret, other.ClientSecret);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is AzureEnvironmentParams other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TenantId != null ? TenantId.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ SubscriptionId.GetHashCode();
                hashCode = (hashCode * 397) ^ (ResourceGroup != null ? ResourceGroup.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ClientId.GetHashCode();
                hashCode = (hashCode * 397) ^ (ClientSecret != null ? ClientSecret.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}