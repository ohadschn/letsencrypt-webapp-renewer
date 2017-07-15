using LetsEncrypt.Azure.Core.Models;

namespace OhadSoft.AzureLetsEncrypt.Renewal
{
    internal class AuthProviderConfig : IAuthorizationChallengeProviderConfig
    {
        public bool DisableWebConfigUpdate => false;
    }
}