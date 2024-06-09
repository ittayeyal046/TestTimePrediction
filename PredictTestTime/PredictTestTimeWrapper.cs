using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PredictTestTimeWrapper
{
    /// <summary>
    /// The predict test time wrapper.
    /// </summary>
    public class PredictTestTimeWrapper
    {
        private readonly string pythonExePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="PredictTestTimeWrapper"/> class.
        /// </summary>
        /// <param name="pythonExePath">The python exe path.</param>
        public PredictTestTimeWrapper(string pythonExePath)
        {
            this.pythonExePath = pythonExePath;
        }

        /// <summary>
        /// Predicts the
        /// </summary>
        /// <param name="parametersDictionary">The parameters dictionary.</param>
        /// <returns>A TimeSpan.</returns>
        public TimeSpan Predict(IDictionary<string, string> parametersDictionary)
        {
            // Path to the Python script you want to run
            string predictTestTimeScriptName = @"PredictTestTime.py";
            string appFolder = AppDomain.CurrentDomain.BaseDirectory;

            // Create a ProcessStartInfo object
            var parameters = string.Join(" ", JsonConvert.SerializeObject(parametersDictionary));
            var fixParametersForPython = parameters.Replace("\"", "\"\"");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = $"{pythonExePath}",
                Arguments = $"{appFolder}{predictTestTimeScriptName} \"{fixParametersForPython}\"",
                WorkingDirectory = Path.GetDirectoryName(pythonExePath),
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Start the Python process
            using var process = new Process { StartInfo = psi };
            process.Start();

            // Read the output of the Python process
            var outputLine = process.StandardOutput.ReadToEnd();
            var errorOutput = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (outputLine == null || string.IsNullOrEmpty(outputLine))
                throw new InvalidDataException(errorOutput);

            // look for number the seconds after output:
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
                return TimeSpan.Zero;
            }
        }
    }
}
