using System;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public sealed class GoDaddyEnvironmentParams : IEquatable<GoDaddyEnvironmentParams>
    {
        public bool AllowNull { get; }

        public string ApiKey { get; }
        public string ApiSecret { get; }
        public string Domain { get; }
        public string ShopperId { get; }

        public GoDaddyEnvironmentParams(string apiKey, string apiSecret, string domain, string shopperId, bool allowNull = false)
        {
            AllowNull = allowNull;

            ApiKey = ValidateParam(apiKey, nameof(apiKey), ParamValidator.VerifyString);
            ApiSecret = ValidateParam(apiSecret, nameof(apiSecret), ParamValidator.VerifyString);
            Domain = ValidateParam(domain, nameof(domain), ParamValidator.VerifyString);
            ShopperId = ValidateParam(shopperId, nameof(shopperId), ParamValidator.VerifyString);
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

        public override string ToString()
        {
            return Invariant($"{nameof(ApiKey)}: {ApiKey}, {nameof(Domain)}: {Domain}, {nameof(ShopperId)}: {ShopperId}");
        }

        public bool Equals(GoDaddyEnvironmentParams other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(ApiKey, other.ApiKey, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(ApiSecret, other.ApiSecret, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(Domain, other.Domain, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(ShopperId, other.ShopperId, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is GoDaddyEnvironmentParams other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ApiKey != null ? ApiKey.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (ApiSecret != null ? ApiSecret.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ShopperId != null ? ShopperId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}