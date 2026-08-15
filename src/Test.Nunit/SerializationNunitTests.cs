namespace Test.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NUnit.Framework;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// NUnit adapter over the shared Touchstone suites. Each Touchstone test case
    /// is surfaced as an individual NUnit test via a TestCaseSource, executed
    /// through the shared descriptor's delegate so the test logic lives only in
    /// Test.Shared.
    /// </summary>
    [TestFixture]
    public sealed class SerializationNunitTests
    {
        /// <summary>
        /// One NUnit test per Touchstone test case across all suites.
        /// </summary>
        public static IEnumerable TestCases => new TouchstoneTestCaseSource(SerializationSuites.All);

        /// <summary>
        /// Execute a single Touchstone test case.
        /// </summary>
        /// <param name="testCase">Test case descriptor.</param>
        [TestCaseSource(nameof(TestCases))]
        public async Task Run(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
