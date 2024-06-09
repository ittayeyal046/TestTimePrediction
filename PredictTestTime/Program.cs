using System.Diagnostics;
using Newtonsoft.Json;
using PredictTestTimeWrapper;


namespace PythonProcessExample
{
    class Program
    {
        static void Main(string[] args)
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
            var ttpWrapper = new PredictTestTimeWrapper.PredictTestTimeWrapper(pythonExe);
            
            var result = ttpWrapper.Predict(parameters);
            Console.WriteLine(result);
        }
    }
}