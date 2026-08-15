namespace Test.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::Xunit;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.XunitAdapter;

    /// <summary>
    /// xUnit adapter over the shared Touchstone suites. Each Touchstone test case
    /// is surfaced as an individual xUnit theory row, executed through the shared
    /// descriptor's delegate so the test logic lives only in Test.Shared.
    /// </summary>
    public sealed class SerializationXunitTests
    {
        /// <summary>
        /// One theory row per Touchstone test case across all suites.
        /// </summary>
        public static TheoryData<TestCaseDescriptor> TestCases => new TouchstoneTheoryData(SerializationSuites.All);

        /// <summary>
        /// Execute a single Touchstone test case.
        /// </summary>
        /// <param name="testCase">Test case descriptor.</param>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task Run(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
