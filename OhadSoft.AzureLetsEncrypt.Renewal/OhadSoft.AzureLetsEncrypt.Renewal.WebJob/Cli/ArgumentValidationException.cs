using System;
using System.Diagnostics.CodeAnalysis;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Cli
{
    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Justification = "Internal exception")]
    [SuppressMessage("Sonar", "S3871:Exception types should be public", Justification = "Internal exception")]
    [Serializable]
    internal class ArgumentValidationException : Exception
    {
        public string HelpText { get; }

        public ArgumentValidationException(string message, string helpText)
            : base(message)
        {
            HelpText = helpText;
        }
    }
}