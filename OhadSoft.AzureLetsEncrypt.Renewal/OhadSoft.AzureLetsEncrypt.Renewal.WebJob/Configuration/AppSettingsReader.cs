using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Configuration
{
    public class AppSettingsReader : IAppSettingsReader
    {
        public string GetStringOrDefault(string key, string defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }

        public string GetString(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetStringOrDefault(key) ?? throw new ConfigurationErrorsException(FormattableString.Invariant($"Missing configuration '{key}'"));
        }

        public Guid GetGuidOrDefault(string key, Guid defaultValue = default(Guid))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Guid.TryParse(GetStringOrDefault(key), out Guid guid) ? guid : defaultValue;
        }

        public Guid GetGuid(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Guid.TryParse(GetStringOrDefault(key), out Guid guid)
                ? guid
                : throw new ConfigurationErrorsException(FormattableString.Invariant($"Configuration value for key '{key}' was not found or could not be parsed as GUID"));
        }

        public IReadOnlyList<string> GetDelimitedListOrDefault(string key, char delimiter = ';', IReadOnlyList<string> defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetStringOrDefault(key)?.Split(delimiter).Select(s => s.Trim()).ToArray() ?? defaultValue;
        }

        public IReadOnlyList<string> GetDelimitedList(string key, char delimiter = ';')
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetDelimitedListOrDefault(key, delimiter) ??
                   throw new ConfigurationErrorsException(FormattableString.Invariant($"Missing configuration '{key}' (expected '{delimiter}'-delimited list)"));
        }

        public bool GetBooleanOrDefault(string key, bool defaultValue = false)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Boolean.TryParse(GetStringOrDefault(key), out bool b) ? b : defaultValue;
        }

        public bool GetBoolean(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Boolean.TryParse(GetStringOrDefault(key), out bool b)
                ? b
                : throw new ConfigurationErrorsException(FormattableString.Invariant($"Configuration value for key '{key}' was not found or could not be parsed as boolean"));
        }

        public int GetInt32OrDefault(string key, int defaultValue = 0)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Int32.TryParse(GetStringOrDefault(key), out int i) ? i : defaultValue;
        }

        public int GetInt32(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Int32.TryParse(GetStringOrDefault(key), out int i)
                ? i
                : throw new ConfigurationErrorsException(FormattableString.Invariant($"Configuration value for key '{key}' was not found or could not be parsed as int"));
        }

        public Uri GetUriOrDefault(string key, UriKind uriKind = UriKind.Absolute, Uri defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Uri.TryCreate(GetStringOrDefault(key), uriKind, out Uri uri) ? uri : defaultValue;
        }

        public Uri GetUri(string key, UriKind uriKind = UriKind.Absolute)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetUriOrDefault(key, uriKind) ??
                   throw new ConfigurationErrorsException(FormattableString.Invariant($"Configuration value for key '{key}' was not found or could not be parsed as Uri ({uriKind})"));
        }

        public string GetConnectionStringOrDefault(string key, string defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return ConfigurationManager.ConnectionStrings[key]?.ConnectionString ?? defaultValue;
        }

        public string GetConnectionString(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetConnectionStringOrDefault(key)
                ?? throw new ConfigurationErrorsException(FormattableString.Invariant($"Missing connection string '{key}'"));
        }
    }
}