using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.FormattableString;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests.Util
{
    public static class AssertExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Test Assertion Helper")]
        public static void Throws<T>(Action action, Predicate<T> predicate = null)
            where T : Exception
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            try
            {
                action();
            }
            catch (T e)
            {
                if (predicate == null || predicate(e))
                {
                    return;
                }

                Assert.Fail(Invariant($"Exception of type {typeof(T)} thrown as expected, but the provided predicate rejected it: {e}"));
            }
            catch (Exception e)
            {
                Assert.Fail(Invariant($"Expected exception of type {typeof(T)} but a different exception was thrown: {e}"));
            }

            Assert.Fail(Invariant($"No exception thrown, expected {typeof(T)}"));
        }
    }
}