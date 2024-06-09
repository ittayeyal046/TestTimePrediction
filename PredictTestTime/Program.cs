namespace PredictTestTimeWrapper;

/// <summary>
/// The program.
/// </summary>
public class Program
{
    /// <summary>
    /// Main
    /// </summary>
    /// <param name="args">The args.</param>
    public static void Main(string[] args)
    {
        // Path to the Python executable
        var pythonExe = args[0];

        var parameters = new Dictionary<string, string>()
            {
                { "IsConcurrent", "False" },
                { "Patterns_Count", "3000" },
                { "Tests_Count", "1000" },
                { "Mtt_Count", "10" },
                { "ConcurrentFlows_Count", "50" },
                { "Shmoo_tests_count", "10" },
                { "PartType", "224LNE4VB" },
                { "ProcessStep", "CLASSHOT" },
                { "ExperimentType", "Engineering" }
            };
        var ttpWrapper = new global::PredictTestTimeWrapper.PredictTestTimeWrapper(pythonExe);

        var result = ttpWrapper.Predict(parameters);
        Console.WriteLine(result);
    }
}
