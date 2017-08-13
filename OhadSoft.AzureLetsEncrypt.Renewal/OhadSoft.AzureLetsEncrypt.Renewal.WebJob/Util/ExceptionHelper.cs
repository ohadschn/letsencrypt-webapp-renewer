using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Util
{
    public static class ExceptionHelper
    {
        public static bool IsCriticalException(Exception ex)
        {
            return ex is OutOfMemoryException || ex is ThreadAbortException || ex is AccessViolationException || ex is SEHException;
        }
    }
}