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
            //var pythonExe = @"C:\Users\ittayeya\AppData\Local\Programs\Python\Python312\python.exe";
            var pythonExe = @"C:\ProgramData\Anaconda3\python.exe";

            var parameters = new Dictionary<string, string>()
            {
                { "IsConcurrent", "True" }, 
                { "Patterns_Count", "3000" }, 
                { "Tests_Count", "1000" }, 
                { "Mtt_Count", "100" }, 
                { "ConcurrentFlows_Count", "5" }, 
                { "Shmoo_tests_count", "10" }, 
                { "PartType", "H64ADNSVAL" }, 
                { "ProcessStep", "CLASSCOLD" }, 
                { "ExperimentType", "Engineering" }
            };
            var ttpWrapper = new TTPWrapper(pythonExe);
            
            var result = ttpWrapper.Predict(parameters);
            Console.WriteLine(result);
        }
    }
}