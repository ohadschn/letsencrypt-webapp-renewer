using System;
using System.Collections.Generic;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings
{
    public interface IAppSettingsReader
    {
        string GetString(string key);
        string GetStringOrDefault(string key, string defaultValue = null);
        Guid GetGuid(string key);
        Guid? GetGuidOrDefault(string key, Guid? defaultValue = null);
        IReadOnlyList<string> GetDelimitedList(string key, char delimiter = ';');
        bool GetBoolean(string key);
        bool? GetBooleanOrDefault(string key, bool? defaultValue = null);
        int GetInt32(string key);
        int? GetInt32OrDefault(string key, int? defaultValue = null);
        Uri GetUri(string key, UriKind uriKind = UriKind.Absolute);
        Uri GetUriOrDefault(string key, UriKind uriKind = UriKind.Absolute, Uri defaultValue = null);
        string GetConnectionString(string key);
        string GetConnectionStringOrDefault(string key, string defaultValue = null);
    }
}