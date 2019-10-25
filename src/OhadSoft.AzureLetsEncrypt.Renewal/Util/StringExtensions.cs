using System;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Util
{
    // https://stackoverflow.com/a/444818/67824
    public static class StringExtensions
    {
        public static bool Contains(this string source, string substring, StringComparison stringComparison)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.IndexOf(substring, stringComparison) >= 0;
        }
    }
}