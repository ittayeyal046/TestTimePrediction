﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using CsvHelper;
using Serilog;
using Trace.Api.Common;
using Trace.Api.Configuration;

namespace PrepareTrainingData
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var appDirectory = GetAppDirectory();

            var fileName = @$"ITuffProcessedData";
            var dataFileName = appDirectory + $"\\{fileName}.csv";
            var logFileName = appDirectory + "\\Logs\\" + $"{fileName}.{DateTime.Now:yy-MM-dd_hh-mm-ss}.log.txt";

            var logger = new LoggerConfiguration()
                       .WriteTo.Console()
                       .WriteTo.File(logFileName)
                       .CreateLogger();
            var sw = new Stopwatch();

            try
            {
                logger.Information("Main starting...");
                sw.Start();

                // object containing IDC network drives map
                var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);

                var traceParser = new ITuffServices(logger);

                var allItuffDefinitions = traceParser.GetClassITuffDefinitions().ToArray();

                var lastITuffRecordDate = GetLastRecordDate(dataFileName);
                var ituffListForParsing =
                    allItuffDefinitions
                        .Where(ituff => ituff.EndDate > lastITuffRecordDate)
                        .Where(ituff => ituff.ExperimentType is "Engineering" or "Correlation" or "WalkTheLot")
                        .OrderBy(ituff => ituff.EndDate)
                        .TakeLast(allItuffDefinitions.Count());

                IDataProvider dataProvider = new DataProvider();

                var csv = new CsvWriter(dataFileName);
                foreach (var ituffDefinitionGroup in
                         ituffListForParsing.GroupBy(i => i.StplPath + "_" + i.TplPath))
                {
                    var testProgram = traceParser.GetTestProgram(
                        driveMapping,
                        ituffDefinitionGroup.First().StplPath,
                        ituffDefinitionGroup.First().TplPath);

                    if (testProgram == null)
                    {
                        continue;
                    }

                    foreach (var ituffDefinition in ituffDefinitionGroup)
                    {
                        var newRecords = await dataProvider.FillRecordsAsync(
                            driveMapping,
                            traceParser,
                            ituffDefinition,
                            testProgram);

                        try
                        {
                            csv.WriteRecords(newRecords);
                        }
                        catch // in case file is already open in excel
                        {
                            continue;
                        }

                        logger.Information($"Writing {newRecords.Count()} records to file {dataFileName}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed with exception {e}");
                throw;
            }
            finally
            {
                sw.Stop();
                logger.Information($"\nProgram run took {sw.Elapsed}");
            }
        }

        private static DateTime GetLastRecordDate(string dataFileName)
        {
            if (!File.Exists(dataFileName))
            {
                return DateTime.MinValue;
            }

            string lastLine = File.ReadLines(dataFileName).Last(l => !string.IsNullOrEmpty(l));
            Regex regex = new Regex("(\\d{1,2}/\\d{1,2}/\\d{4} \\d{1,2}:\\d{2}:\\d{2} [AP]M)");
            var dateTime = regex.Match(lastLine).Value;
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
