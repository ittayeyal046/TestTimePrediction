namespace TTPService.Configuration;

internal class PythonPathProvider : IPythonPathProvider
{
    public string PythonPath { get; }

    public PythonPathProvider(string pythonPath)
    {
        PythonPath = pythonPath;
    }
}