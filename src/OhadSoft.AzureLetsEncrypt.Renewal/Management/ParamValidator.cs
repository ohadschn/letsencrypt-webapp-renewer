using System;
using System.Collections.Generic;
using System.Linq;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management
{
    public static class ParamValidator
    {
        public static string VerifyOptionalHostName(string hostName, string name)
        {
            return hostName == null || Uri.CheckHostName(hostName) != UriHostNameType.Unknown
                ? hostName
                : throw new ArgumentException("Internet host name must be valid", name);
        }

        public static int VerifyPositiveInteger(int number, string name)
        {
            return number > 0
                ? number
                : throw new ArgumentException("Integer must be positive", name);
        }

        public static string VerifyOptionalString(string str, string name)
        {
            return str == null || !str.All(Char.IsWhiteSpace)
                ? str
                : throw new ArgumentException("String must be either null or non-whitespace", name);
        }

        public static IReadOnlyList<string> VerifyHosts(IReadOnlyList<string> hosts, bool enforceWildcard, string name)
        {
            if (hosts == null || hosts.Count == 0 || hosts.Any(h => Uri.CheckHostName(h?.Replace('*', 'x')) == UriHostNameType.Unknown))
            {
                throw new ArgumentException("Host collection must be non-null, contain at least one element, and contain valid host names", name);
            }

            if (enforceWildcard && hosts.Any(h => !h.StartsWith("*.", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Only wildcard host names are supported for the DNS challenge (must begin with '*.'", name);
            }

            return hosts;
        }

        public static string VerifyEmail(string email, string name)
        {
            return !String.IsNullOrWhiteSpace(email) && email.Contains("@") && email.Length >= 3 && email.Length <= 254
                   && !email.StartsWith("@", StringComparison.OrdinalIgnoreCase) && !email.EndsWith("@", StringComparison.OrdinalIgnoreCase)
                ? email
                : throw new ArgumentException("E-mail address must not be null and must be valid", name);
        }

        public static string VerifyString(string str, string name)
        {
            return String.IsNullOrWhiteSpace(str)
                ? throw new ArgumentException("String cannot be null or whitespace", name)
                : str;
        }

        public static Guid VerifyGuid(Guid guid, string name)
        {
            return guid != default
                ? guid
                : throw new ArgumentException("GUID cannot be empty", name);
        }

        public static Uri VerifyOptionalUri(Uri uri, string name)
        {
            return uri == null || uri.IsAbsoluteUri
                ? uri
                : throw new ArgumentException("URI must be either null or absolute", name);
        }

        public static T VerifyNonNull<T>(T value, string name)
            where T : class
        {
            return value ?? throw new ArgumentNullException(name);
        }
    }
}