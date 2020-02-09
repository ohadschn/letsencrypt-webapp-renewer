using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Util
{
    public static class ExceptionHelper
    {
        private static int s_debugMode = 0;
        public static bool DebugMode
        {
            get => Interlocked.CompareExchange(ref s_debugMode, 100, 100) == 1;
            set => Interlocked.Exchange(ref s_debugMode, value ? 1 : 0);
        }

        public static bool IsCriticalException(Exception ex)
        {
            Trace.TraceInformation("Evaluating exception: {0} ({1})", ex, DebugMode);
            return DebugMode || ex is OutOfMemoryException || ex is ThreadAbortException || ex is AccessViolationException || ex is SEHException;
        }
    }
}