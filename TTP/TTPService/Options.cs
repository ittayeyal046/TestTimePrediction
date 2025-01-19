using CommandLine;

namespace TTPService;

public class Options
{
    [Option('p', "PythonPath", Required = true, HelpText = "Path to the Python executable.")]
    public string PythonPath { get; set; }
    
    [Option('a', "ApplicationUrl", Default = "http://*:54487", Required = false, HelpText = "Path to the Python executable.")]
    public string ApplicationUrl { get; set; }
}