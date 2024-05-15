
using System.Diagnostics;
using Serilog;
using TestTimePrediction;
using Trace.Api.Common;
using Trace.Api.Configuration;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var appDirectory = GetAppDirectory();

            var fileName = @$"ITuffProcessedData";
            var dataFileName = appDirectory + $"\\{fileName}.csv";
            var logFileName = appDirectory + "\\Logs\\" + $"\\{fileName}_{DateTime.Now:yy-MM-dd_hh-mm-ss}.log.txt";

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
                    .OrderBy(ituff => ituff.EndDate)
                    .TakeLast(allItuffDefinitions.Count());

            IDataCreator dataCreator = new DataCreator();

            var csv = new Csv(dataFileName);
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
                    var newRecords = await dataCreator.FillRecordsAsync(driveMapping, traceParser, ituffDefinition, testProgram);

                    try
                    {
                        csv.Write(newRecords);
                    }
                    // in case file is already open in excel
                    catch
                    {
                        continue;
                    }

                    logger.Information($"Writing {newRecords.Count()} records to file {dataFileName}");
                }
            }

            sw.Stop();
            logger.Information($"\nProgram run took {sw.Elapsed}");
        }

        private static DateTime GetLastRecordDate(string dataFileName)
        {
            if(!File.Exists(dataFileName))
                return DateTime.MinValue;

            string lastLine = File.ReadLines(dataFileName).Last(l => !string.IsNullOrEmpty(l));
            var dateTime = lastLine.Split(',').ElementAt(12);
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
