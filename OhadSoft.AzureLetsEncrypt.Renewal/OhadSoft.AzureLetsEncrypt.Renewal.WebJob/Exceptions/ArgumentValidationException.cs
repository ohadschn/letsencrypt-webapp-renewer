using System;
using System.Diagnostics.CodeAnalysis;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Exceptions
{
    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Justification = "Internal exception")]
    [SuppressMessage("Sonar", "S3871:Exception types should be public", Justification = "Internal exception")]
    [Serializable]
    internal class ArgumentValidationException : ArgumentException
    {
        public ArgumentValidationException(ArgumentException argumentException)
            : base(argumentException?.Message, argumentException?.ParamName, argumentException)
        {
        }
    }
}