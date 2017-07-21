using LetsEncrypt.Azure.Core.Models;

namespace OhadSoft.AzureLetsEncrypt.Renewal.Configuration
{
    internal class AuthProviderConfig : IAuthorizationChallengeProviderConfig
    {
        public bool DisableWebConfigUpdate => false;
    }
}