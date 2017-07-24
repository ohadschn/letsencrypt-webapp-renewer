using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests
{
    public static class AssertExtensions
    {
        public static void Throws<T>(Action action, Predicate<T> predicate = null) where T : Exception
        {
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

                Assert.Fail($"Exception of type {typeof(T)} thrown as expected, but the provided predicate rejected it: {e}");
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected exception of type {typeof(T)} but a different exception was thrown: {e}");
            }

            Assert.Fail($"No exception thrown, expected {typeof(T)}");
        }
    }
}