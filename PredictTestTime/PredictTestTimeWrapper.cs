using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PredictTestTimeWrapper
{
    public class PredictTestTimeWrapper
    {
        private readonly string pythonExePath;

        public PredictTestTimeWrapper(string pythonExePath)
        {
            this.pythonExePath = pythonExePath;
        }

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

        private static string GetAppDirectory()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Create a subfolder within LocalAppData where you want to save your files
            string subfolder = @"TTP\TrainingData";
            string fullPath = Path.Combine(localAppDataPath, subfolder);

            // Create the directory if it doesn't exist
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return fullPath;
        }

    }
}
