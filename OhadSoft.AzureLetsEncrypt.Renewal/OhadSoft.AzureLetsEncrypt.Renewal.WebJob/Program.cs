using System;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                new Renewer().RenewWebAppCertFromConfiguration(new RenewalManager(), new ConfigurationHelper());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception:" + e);
                throw; // we want the webjob to fail
            }
        }
    }
}