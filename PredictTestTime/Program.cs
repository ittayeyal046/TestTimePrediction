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
            var pythonExe = @"C:\Python33\python.exe";
            var input = new Dictionary<string, string>() { { "Name", "ittay" }, { "Id", "1234567" } };
            var ttpWrapper = new TTPWrapper(pythonExe);
            
            var result = ttpWrapper.Predict(input);
            Console.WriteLine(result);
        }
    }
}