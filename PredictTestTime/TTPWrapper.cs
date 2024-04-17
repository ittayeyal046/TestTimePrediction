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
    public class TTPWrapper
    {
        private readonly string pythonExePath;

        public TTPWrapper(string pythonExePath)
        {
            this.pythonExePath = pythonExePath;
        }

        public TimeSpan Predict(IDictionary<string, string> parametersDictionary)
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

            var prediction = TimeSpan.FromSeconds(double.Parse(output));
            return prediction;
        }
    }
}
