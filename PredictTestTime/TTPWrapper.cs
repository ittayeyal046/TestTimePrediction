using System.Diagnostics;
using System.Text.RegularExpressions;
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
                WorkingDirectory = Path.GetDirectoryName(pythonExePath),
                RedirectStandardOutput = true
            };

            // Start the Python process
            using var process = new Process { StartInfo = psi };
            process.Start();

            // Read the output of the Python process
            var outputLine = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (outputLine == null)
                throw new InvalidDataException();

            string pattern = @"output:\s*(\d+)";
            Match match = Regex.Match(outputLine, pattern);

            if (match.Success)
            {
                string numberString = match.Groups[1].Value;
                var prediction = TimeSpan.FromSeconds(double.Parse(numberString));
                return prediction;
            }
            else
            {
                return  TimeSpan.Zero;
            }
        }
    }
}
