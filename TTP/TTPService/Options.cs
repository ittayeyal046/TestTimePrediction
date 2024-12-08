using CommandLine;

namespace TTPService;

public class Options
{
    [Option('p', "PythonPath", Required = true, HelpText = "Path to the Python executable.")]
    public string PythonPath { get; set; }
}