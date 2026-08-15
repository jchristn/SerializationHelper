namespace Test.Automated
{
    using System.Threading.Tasks;
    using Test.Shared;
    using Touchstone.Cli;

    /// <summary>
    /// Touchstone CLI runner for the SerializationHelper test suites. Runs every
    /// suite defined in Test.Shared and returns a non-zero exit code if any test
    /// fails, making it suitable for CI. Pass an optional path argument to export
    /// results as JSON.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Optional single argument: path to a JSON results file.</param>
        /// <returns>Process exit code (0 on success).</returns>
        public static async Task<int> Main(string[] args)
        {
            string resultsPath = args != null && args.Length > 0 ? args[0] : null;
            return await ConsoleRunner.RunAsync(SerializationSuites.All, resultsPath: resultsPath);
        }
    }
}
