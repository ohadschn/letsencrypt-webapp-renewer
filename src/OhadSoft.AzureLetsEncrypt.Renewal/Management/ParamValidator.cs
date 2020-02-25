using System;
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