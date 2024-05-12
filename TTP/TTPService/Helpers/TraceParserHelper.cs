using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using TestTimePrediction;
using Trace.Api.Common;
using Trace.Api.Common.TP;
using Trace.Api.Configuration;

namespace TTPService.Helpers;

public class TraceParserHelper : ITraceParserHelper
{
    public TestProgram ParseTP(string stplPath, string tplPath)
    {
        var appDirectory = GetAppDirectory();

        var fileName = @$"TraceParserHelper_{DateTime.Now:yy-MM-dd_hh-mm-ss}";
        var logFileName = appDirectory + $"\\{fileName}.log.txt";

        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(logFileName)
            .CreateLogger();

        logger.Information("starting to parse TP from TraceParser...");

        // object containing IDC network drives map
        var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);
        var traceParser = new TraceParser(logger);

        TestProgram testProgram = traceParser.GetTestProgram(driveMapping, stplPath, tplPath);

        if (testProgram == null)
        {
            logger.Error("Failed to parse the Test Program from TraceApi");
            return null;
        }

        return testProgram;
    }

    public IEnumerable<T> GetAllElement<T>(TestProgram testProgram) where T : TpItemBase
    {
        return testProgram?.DeepSelect<T>(true);
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