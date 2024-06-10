using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PredictTestTimeWrapper
{
    /// <summary>
    /// The predict test time wrapper.
    /// </summary>
    public class PredictTestTimeWrapper : IPredictTestTimeWrapper
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
        /// <param name="parametersDictionary">The parameters' dictionary.</param>
        /// <returns>A TimeSpan.</returns>
        public async Task<TimeSpan> Predict(IDictionary<string, string> parametersDictionary)
        {
            // Path to the Python script you want to run
            var predictTestTimeScriptName = @"PredictTestTime.py";
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;

            // Create a ProcessStartInfo object
            var parameters = string.Join(" ", JsonConvert.SerializeObject(parametersDictionary));
            var fixParametersForPython = parameters.Replace("\"", "\"\"");

            var psi = new ProcessStartInfo
            {
                FileName = $"{pythonExePath}",
                Arguments = $"{appFolder}{predictTestTimeScriptName} \"{fixParametersForPython}\"",
                WorkingDirectory = Path.GetDirectoryName(pythonExePath),
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Start the Python process
            using var process = new Process();
            process.StartInfo = psi;
            process.Start();

            // Read the output of the Python process
            var outputLine = await process.StandardOutput.ReadToEndAsync();
            var errorOutput = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (outputLine == null || string.IsNullOrEmpty(outputLine))
            {
                throw new InvalidDataException(errorOutput);
            }

            // look for number the seconds after output:
            const string pattern = @"output:\s*(\d+)";
            var match = Regex.Match(outputLine, pattern);

            if (match.Success)
            {
                string numberString = match.Groups[1].Value;
                var prediction = TimeSpan.FromSeconds(double.Parse(numberString));
                return prediction;
            }
            else
            {
                throw new InvalidDataException("Failed to get test time prediction");
            }
        }
    }
}
