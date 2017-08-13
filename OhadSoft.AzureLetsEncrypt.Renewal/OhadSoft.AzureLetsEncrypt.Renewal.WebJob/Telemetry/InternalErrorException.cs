using System;
using System.Runtime.Serialization;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Telemetry
{
    [Serializable]
    public class InternalErrorException : Exception
    {
        public InternalErrorException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InternalErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}