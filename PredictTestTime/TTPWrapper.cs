using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PredictTestTimeWrapper
{
    internal class TTPWrapper
    {
        private readonly string pythonExePath;

        public TTPWrapper(string pythonExePath)
        {
            this.pythonExePath = pythonExePath;
        }

        public IDictionary<string, string> Predict(IDictionary<string, string> parametersDictionary)
        {
            // Path to the Python script you want to run
            string pythonScript = @"PredictTestTime.py";

            // Create a ProcessStartInfo object
            var parameters = string.Join(" ", JsonConvert.SerializeObject(parametersDictionary));
            var fixParametersForPython = parameters.Replace("\"", "\"\"");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = $"{pythonExePath}",
                Arguments = $"\"{Path.GetFullPath(".")}\\{pythonScript}\" \"{fixParametersForPython}\"",
                RedirectStandardOutput = true
            };

            // Start the Python process
            using var process = new Process { StartInfo = psi };
            process.Start();

            // Read the output of the Python process
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (output == null)
                throw new InvalidDataException();

            var ttpOutputDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(output);

            if (ttpOutputDictionary != null)
            {
                Console.WriteLine($"Python output:");

                foreach (var kvp in ttpOutputDictionary)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                }
            }

            return ttpOutputDictionary;
        }
    }
}
