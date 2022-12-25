// See https://aka.ms/new-console-template for more information

using TestTimePrediction;
using Trace.Api.Common.TP;
using Trace.Api.Common;
using Trace.Api.Configuration;
using Trace.Api.Services.TestProgramParser;
using Trace.Api.Services.TestProgramParser.Interfaces;
using Trace.Api.Services.Common;
using Trace.Api.Services.TestResults.ItuffIndex;

Console.WriteLine("Starting...");

var traceParser = new TraceParser();
var testProgram = traceParser.Parse();

Console.ReadLine();