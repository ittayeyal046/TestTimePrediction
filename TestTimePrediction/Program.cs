
using System.Diagnostics;
using Serilog;
using TestTimePrediction;
using Trace.Api.Common;
using Trace.Api.Common.Helpers;
using Trace.Api.Common.Ituff;
using Trace.Api.Common.TP;
using Trace.Api.Configuration;
using Trace.Api.Services.Common;
using Trace.Api.Services.TestResults.ItuffIndex;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var fileName = @$"C:\Temp\TestPredictionResults_{DateTime.Now:yy-MM-dd_hh-mm-ss}";
            var fileNameCsv = fileName + ".csv";
            var fileNameLog = fileName + ".log.txt";

            var logger = new LoggerConfiguration()
                       .WriteTo.Console()
                       .WriteTo.File(fileNameLog)
                       .CreateLogger();

            logger.Information("Main starting...");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (args.Length > 1)
            {
                logger.Error("Number of arguments must be 0 or 1 (0 use all iTuffs, 1 define amount of iTuffs)");
                return;
            }

            // object containing IDC network drives map
            var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);

            var traceParser = new TraceParser(logger);

            var allItuffDefinitions = traceParser.GetClassITuffDefinitions();
            
            int numOfItuff = args.Length == 0 ? -1 : int.Parse(args[0]);
            var ituffsForParsing = allItuffDefinitions.OrderBy(ituff => ituff.EndDate)
                .TakeLast(numOfItuff == -1 ? allItuffDefinitions.Count() : numOfItuff);

            IDataCreator dataCreator = new DataCreator();
            List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();
            
            foreach (var ituffDefinitionGroup in 
                                          ituffsForParsing.GroupBy(i => i.StplPath+"_"+i.TplPath))
            {
                TestProgram testProgram = traceParser.GetTestProgram(driveMapping, ituffDefinitionGroup.First().StplPath, ituffDefinitionGroup.First().TplPath);

                if (testProgram == null)
                {
                    continue;
                }

                foreach (var ituffDefinition in ituffDefinitionGroup)
                {
                    records.AddRange(dataCreator.FillRecords(driveMapping, traceParser, ituffDefinition, testProgram));

                    try
                    {
                        if(File.Exists(fileNameCsv))
                            File.Delete(fileNameCsv);

                        var csv = new Csv();
                        csv.Write(fileNameCsv, records);
                    }
                    // in case file is already open in excel
                    catch
                    {
                        continue;
                    }

                    logger.Information($"Writing {records.Count} records to file {fileNameCsv}");
                }
            }

            sw.Stop();
            logger.Information($"\nProgram run took {sw.Elapsed}");
        }
    }
}
