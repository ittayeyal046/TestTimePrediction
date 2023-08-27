using System.Diagnostics;
using Newtonsoft.Json;


namespace PythonProcessExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Path to the Python executable
            string pythonExe = @"C:\Users\ittayeya\Anaconda3\python.exe";

            // Path to the Python script you want to run
            string pythonScript = "PythonTest.py";

            // Create a ProcessStartInfo object
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"{pythonScript} {string.Join(" ", args)}",
                RedirectStandardOutput = true
            };

            // Start the Python process
            using Process? process = Process.Start(psi);

            // Read the output of the Python process
            string? output = process?.StandardOutput.ReadToEnd();
            process?.WaitForExit();

            if (output != null)
            {
                var processOutput = JsonConvert.DeserializeObject<Dictionary<string, object>>(output);

                if (processOutput != null)
                {
                    Console.WriteLine($"Python output:");

                    foreach (var kvp in processOutput)
                    {
                        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                    }
                }
            }
        }
    }
}