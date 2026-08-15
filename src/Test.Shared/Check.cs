namespace Test.Shared
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Lightweight assertion helper for Touchstone descriptor-based tests.
    /// Each method throws <see cref="TestAssertionException"/> on failure, which
    /// Touchstone records as a failed test.
    /// </summary>
    public static class Check
    {
        /// <summary>
        /// Assert that a condition is true.
        /// </summary>
        public static void True(bool condition, string message = null)
        {
            if (!condition) throw new TestAssertionException(message ?? "Expected condition to be true, but it was false.");
        }

        /// <summary>
        /// Assert that a condition is false.
        /// </summary>
        public static void False(bool condition, string message = null)
        {
            if (condition) throw new TestAssertionException(message ?? "Expected condition to be false, but it was true.");
        }

        /// <summary>
        /// Assert that a reference is null.
        /// </summary>
        public static void Null(object value, string message = null)
        {
            if (value != null) throw new TestAssertionException(message ?? "Expected null, but value was '" + value + "'.");
        }

        /// <summary>
        /// Assert that a reference is not null.
        /// </summary>
        public static void NotNull(object value, string message = null)
        {
            if (value == null) throw new TestAssertionException(message ?? "Expected a non-null value, but it was null.");
        }

        /// <summary>
        /// Assert that two values are equal using the default equality comparer.
        /// </summary>
        public static void Equal<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new TestAssertionException(
                    (message ?? "Values are not equal.") +
                    " Expected: '" + Format(expected) + "', Actual: '" + Format(actual) + "'.");
            }
        }

        /// <summary>
        /// Assert that two values are not equal.
        /// </summary>
        public static void NotEqual<T>(T notExpected, T actual, string message = null)
        {
            if (EqualityComparer<T>.Default.Equals(notExpected, actual))
            {
                throw new TestAssertionException(
                    (message ?? "Values should not be equal.") +
                    " Both were: '" + Format(actual) + "'.");
            }
        }

        /// <summary>
        /// Assert that a string contains a substring.
        /// </summary>
        public static void Contains(string haystack, string needle, string message = null)
        {
            if (haystack == null || needle == null || !haystack.Contains(needle))
            {
                throw new TestAssertionException(
                    (message ?? "Substring not found.") +
                    " Expected to find '" + needle + "' in '" + haystack + "'.");
            }
        }

        /// <summary>
        /// Assert that a string does not contain a substring.
        /// </summary>
        public static void DoesNotContain(string haystack, string needle, string message = null)
        {
            if (haystack != null && needle != null && haystack.Contains(needle))
            {
                throw new TestAssertionException(
                    (message ?? "Unexpected substring found.") +
                    " Did not expect to find '" + needle + "' in '" + haystack + "'.");
            }
        }

        /// <summary>
        /// Assert that invoking an action throws an exception assignable to <typeparamref name="TException"/>.
        /// Returns the thrown exception for further inspection.
        /// </summary>
        public static TException Throws<TException>(Action action, string message = null) where TException : Exception
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            try
            {
                action();
            }
            catch (TException ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                throw new TestAssertionException(
                    (message ?? "Wrong exception type.") +
                    " Expected '" + typeof(TException).Name + "' but caught '" + ex.GetType().Name + "': " + ex.Message);
            }

            throw new TestAssertionException(
                (message ?? "No exception thrown.") +
                " Expected an exception of type '" + typeof(TException).Name + "' but none was thrown.");
        }

        /// <summary>
        /// Assert that invoking an action throws any exception.
        /// Returns the thrown exception for further inspection.
        /// </summary>
        public static Exception ThrowsAny(Action action, string message = null)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            try
            {
                action();
            }
            catch (Exception ex)
            {
                return ex;
            }

            throw new TestAssertionException(
                (message ?? "No exception thrown.") + " Expected an exception but none was thrown.");
        }

        private static string Format(object value)
        {
            if (value == null) return "(null)";
            return value.ToString();
        }
    }

    /// <summary>
    /// Exception thrown when a <see cref="Check"/> assertion fails.
    /// </summary>
    public class TestAssertionException : Exception
    {
        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="message">Message.</param>
        public TestAssertionException(string message) : base(message)
        {
        }
    }
}
