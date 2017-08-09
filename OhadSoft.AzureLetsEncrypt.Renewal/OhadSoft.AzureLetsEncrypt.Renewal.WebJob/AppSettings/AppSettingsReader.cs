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

        public bool HasSetting(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return m_appSettings[key] != null;
        }

        public bool HasConnectionString(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return m_connectionStrings[key] != null;
        }

        public string GetString(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return m_appSettings[key] ?? throw new ConfigurationErrorsException(Invariant($"Missing configuration '{key}'"));
        }

        public Guid GetGuid(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Guid.TryParse(GetString(key), out Guid guid)
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

            return Boolean.TryParse(GetString(key), out bool b)
                ? b
                : throw new ConfigurationErrorsException(Invariant($"Configuration value for key '{key}' could not be parsed as boolean"));
        }

        public int GetInt32(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Int32.TryParse(GetString(key), out int i)
                ? i
                : throw new ConfigurationErrorsException(Invariant($"Configuration value for key '{key}' could not be parsed as int32"));
        }

        public Uri GetUri(string key, UriKind uriKind = UriKind.Absolute)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Uri.TryCreate(GetString(key), uriKind, out Uri uri)
                ? uri
                : throw new ConfigurationErrorsException(Invariant($"Configuration value for key '{key}' could not be parsed as Uri ({uriKind})"));
        }

        public string GetConnectionString(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return m_connectionStrings[key]?.ConnectionString ?? throw new ConfigurationErrorsException(Invariant($"Missing connection string '{key}'"));
        }
    }
}