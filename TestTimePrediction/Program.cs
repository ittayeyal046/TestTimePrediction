// See https://aka.ms/new-console-template for more information

using System.DirectoryServices;
using System.Drawing.Text;
using TestTimePrediction;
using TestTimePredictionNamespace;
using Trace.Api.Common.TP;
using Trace.Api.Common;
using Trace.Api.Configuration;
using Trace.Api.Services.TestProgramParser;
using Trace.Api.Services.TestProgramParser.Interfaces;
using Trace.Api.Services.Common;
using Trace.Api.Services.TestResults.ItuffIndex;

Console.WriteLine("Starting...");

var traceParser = new TraceParser();
var testProgram = traceParser.GetTestProgram();
var testInstances = traceParser.GetTestInstances();
var csv = new Csv();
var records = new List<TimePredictionRecord>();

foreach (var testInstance in testInstances)
{
    var unitTest = traceParser.GetUnitTestResults(testInstance);
    records.Add(new TimePredictionRecord());
}

csv.Write(@"C:\Temp\TestPredictionResults.csv", records);

Console.ReadLine();