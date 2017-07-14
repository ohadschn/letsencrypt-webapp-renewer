using LetsEncrypt.Azure.Core.Models;

namespace OhadSoft.AzureLetsEncrypt.Renewal
{
    class AuthProviderConfig : IAuthorizationChallengeProviderConfig
    {
        public bool DisableWebConfigUpdate => false;
    }
}