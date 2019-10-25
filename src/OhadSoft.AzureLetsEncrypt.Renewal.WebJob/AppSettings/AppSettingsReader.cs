using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings
{
    public class AppSettingsReader : IAppSettingsReader
    {
        private readonly NameValueCollection m_appSettings;
        private readonly ConnectionStringSettingsCollection m_connectionStrings;

        public AppSettingsReader(NameValueCollection appSettings, ConnectionStringSettingsCollection connectionStrings)
        {
            m_appSettings = appSettings;
            m_connectionStrings = connectionStrings;
        }

        public string GetString(string key)
        {
            return GetStringOrDefault(key) ?? throw new ConfigurationErrorsException(Invariant($"Missing configuration '{key}'"));
        }

        public string GetStringOrDefault(string key, string defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return m_appSettings[key] ?? defaultValue;
        }

        public Guid GetGuid(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return ParseGuid(key, GetString(key));
        }

        public Guid? GetGuidOrDefault(string key, Guid? defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var guidString = GetStringOrDefault(key);
            return guidString == null
                ? defaultValue
                : ParseGuid(key, guidString);
        }

        private static Guid ParseGuid(string key, string guidString)
        {
            return Guid.TryParse(guidString, out Guid guid)
                ? guid
                : throw new ConfigurationErrorsException(Invariant($"Configuration value for key '{key}' could not be parsed as GUID"));
        }

        public IReadOnlyList<string> GetDelimitedList(string key, char delimiter = ';')
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetString(key).Split(delimiter).Select(s => s.Trim()).ToArray();
        }

        public bool GetBoolean(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return ParseBoolean(key, GetString(key));
        }

        public bool? GetBooleanOrDefault(string key, bool? defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var boolString = GetStringOrDefault(key);
            return boolString == null ?
                defaultValue :
                ParseBoolean(key, boolString);
        }

        private static bool ParseBoolean(string key, string boolString)
        {
            return Boolean.TryParse(boolString, out bool b)
                ? b
                : throw new ConfigurationErrorsException(Invariant($"Configuration value for key '{key}' could not be parsed as boolean"));
        }

        public int GetInt32(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return ParseInt32(key, GetString(key));
        }

        public int? GetInt32OrDefault(string key, int? defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var intString = GetStringOrDefault(key);
            return intString == null
                ? defaultValue
                : ParseInt32(key, intString);
        }

        private static int ParseInt32(string key, string intString)
        {
            return Int32.TryParse(intString, out int i)
                ? i
                : throw new ConfigurationErrorsException(Invariant($"Configuration value for key '{key}' could not be parsed as int32"));
        }

        public Uri GetUri(string key, UriKind uriKind = UriKind.Absolute)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return ParseUri(key, GetString(key), uriKind);
        }

        public Uri GetUriOrDefault(string key, UriKind uriKind = UriKind.Absolute, Uri defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var uriString = GetStringOrDefault(key);
            return uriString == null
                ? defaultValue :
                ParseUri(key, uriString, uriKind);
        }

        private static Uri ParseUri(string key, string uriString, UriKind uriKind)
        {
            return Uri.TryCreate(uriString, uriKind, out Uri uri)
                ? uri
                : throw new ConfigurationErrorsException(Invariant($"Configuration value for key '{key}' could not be parsed as Uri ({uriKind})"));
        }

        public string GetConnectionString(string key)
        {
            return GetConnectionStringOrDefault(key) ?? throw new ConfigurationErrorsException(Invariant($"Missing connection string '{key}'"));
        }

        public string GetConnectionStringOrDefault(string key, string defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return m_connectionStrings[key]?.ConnectionString ?? defaultValue;
        }
    }
}