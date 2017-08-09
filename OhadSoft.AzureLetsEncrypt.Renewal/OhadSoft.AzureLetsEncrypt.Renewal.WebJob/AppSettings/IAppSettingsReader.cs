using System;
using System.Collections.Generic;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.AppSettings
{
    public interface IAppSettingsReader
    {
        bool HasSetting(string key);
        bool HasConnectionString(string key);
        string GetString(string key);
        Guid GetGuid(string key);
        IReadOnlyList<string> GetDelimitedList(string key, char delimiter = ';');
        bool GetBoolean(string key);
        int GetInt32(string key);
        Uri GetUri(string key, UriKind uriKind = UriKind.Absolute);
        string GetConnectionString(string key);
    }
}