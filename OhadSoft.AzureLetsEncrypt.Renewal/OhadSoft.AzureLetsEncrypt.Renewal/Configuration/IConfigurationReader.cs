using System;
using System.Collections.Generic;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Configuration
{
    public interface IConfigurationReader
    {
        string GetStringOrDefault(string key, string defaultValue = null);
        string GetString(string key);
        Guid GetGuidOrDefault(string key, Guid defaultValue = default(Guid));
        Guid GetGuid(string key);
        IReadOnlyList<string> GetDelimitedListOrDefault(string key, char delimiter = ';', IReadOnlyList<string> defaultValue = null);
        IReadOnlyList<string> GetDelimitedList(string key, char delimiter = ';');
        bool GetBooleanOrDefault(string key, bool defaultValue = false);
        bool GetBoolean(string key);
        int GetInt32OrDefault(string key, int defaultValue = 0);
        int GetInt32(string key);
        Uri GetUriOrDefault(string key, UriKind uriKind = UriKind.Absolute, Uri defaultValue = null);
        Uri GetUri(string key, UriKind uriKind = UriKind.Absolute);
        string GetConnectionStringOrDefault(string key, string defaultValue = null);
        string GetConnectionString(string key);
    }
}