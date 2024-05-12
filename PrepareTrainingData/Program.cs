
using System.Diagnostics;
using CsvHelper;
using MoreLinq;
using Serilog;
using TestTimePrediction;
using Trace.Api.Common;
using Trace.Api.Common.TP;
using Trace.Api.Configuration;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var appDirectory = GetAppDirectory();

            var fileName = @$"ITuffProcessedData";
            var dataFileName = appDirectory + $"\\{fileName}.csv";
            var logFileName = appDirectory + $"\\{fileName}_{DateTime.Now:yy-MM-dd_hh-mm-ss}.log.txt";

            var logger = new LoggerConfiguration()
                       .WriteTo.Console()
                       .WriteTo.File(logFileName)
                       .CreateLogger();

            logger.Information("Main starting...");
            var sw = new Stopwatch();
            sw.Start();
            
            // object containing IDC network drives map
            var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);

            var traceParser = new TraceParser(logger);

            var allItuffDefinitions = traceParser.GetClassITuffDefinitions().ToArray();

            var lastITuffRecordDate = GetLastRecordDate(dataFileName);
            var ituffListForParsing =
                allItuffDefinitions
                    .Where(ituff => ituff.EndDate > lastITuffRecordDate)
                    .Where(ituff => ituff.ExperimentType is "Engineering" or "Correlation" or "WalkTheLot")
                    .OrderByDescending(ituff => ituff.EndDate);

            IDataCreator dataCreator = new DataCreator();
            List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();

            foreach (var ituffDefinitionGroup in
                                          ituffListForParsing.GroupBy(i => i.StplPath + "_" + i.TplPath))
            {
                var testProgram = traceParser.GetTestProgram(driveMapping, ituffDefinitionGroup.First().StplPath, ituffDefinitionGroup.First().TplPath);

                if (testProgram == null)
                {
                    continue;
                }

                foreach (var ituffDefinition in ituffDefinitionGroup)
                {
                    records.AddRange(dataCreator.FillRecords(driveMapping, traceParser, ituffDefinition, testProgram));

                    try
                    {
                        // if (File.Exists(dataFileName))
                        //     File.Delete(dataFileName);

                        var csv = new Csv();
                        csv.Write(dataFileName, records);
                    }
                    // in case file is already open in excel
                    catch
                    {
                        continue;
                    }

                    logger.Information($"Writing {records.Count} records to file {dataFileName}");
                }
            }

            sw.Stop();
            logger.Information($"\nProgram run took {sw.Elapsed}");
        }

        private static DateTime GetLastRecordDate(string dataFileName)
        {
            string lastLine = File.ReadLines(dataFileName).LastOrDefault();
            var dateTime = lastLine.Split(',').ElementAt(13);
            return DateTime.Parse(dateTime);
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
