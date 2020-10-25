using System;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Management.Util
{
    public static class EmailHelper
    {
        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && email.Contains("@") && email.Length >= 3 && email.Length <= 254
                   && !email.StartsWith("@", StringComparison.OrdinalIgnoreCase) && !email.EndsWith("@", StringComparison.OrdinalIgnoreCase);
        }
    }
}
