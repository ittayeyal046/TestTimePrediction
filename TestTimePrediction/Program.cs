
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
        static async Task Main(string[] args)
        {
            var fileName = @$"C:\Temp\TestPredictionResults_{DateTime.Now:yy-MM-dd_hh-mm-ss}";
            var fileNameCsv = fileName + ".csv";
            var fileNameLog = fileName + ".log";

            var logger = new LoggerConfiguration()
                       .WriteTo.Console()
                       .WriteTo.File(fileNameLog)
                       .CreateLogger();

            logger.Information("Main starting...");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // object containing IDC network drives map
            var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);

            var traceParser = new TraceParser();

            var allItuffDefinitions = traceParser.GetClassITuffDefinitions();
            
            int numOfItuff = -1;
            var ituffsForParsing = allItuffDefinitions.OrderBy(ituff => ituff.EndDate)
                .TakeLast(numOfItuff == -1 ? allItuffDefinitions.Count() : numOfItuff);

            IDataCreator dataCreator = new DataCreator();
            List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();
            
            foreach (var ituffDefinitionGroup in 
                                          ituffsForParsing.GroupBy(i => i.StplPath+"_"+i.TplPath))
            {
                TestProgram testProgram = traceParser.GetTestProgram(driveMapping, ituffDefinitionGroup.First().StplPath, ituffDefinitionGroup.First().TplPath, logger);

                if (testProgram == null)
                {
                    continue;
                }

                foreach (var ituffDefinition in ituffDefinitionGroup)
                {
                    records.AddRange(dataCreator.FillRecords(driveMapping, traceParser, ituffDefinition, testProgram));
                    
                    if(File.Exists(fileName))
                        File.Delete(fileName);

                    var csv = new Csv();
                    csv.Write(fileName, records);

                    logger.Information($"Writing {records.Count} records to file {fileName}");
                }
            }

            sw.Stop();
            logger.Information($"\nProgram run took {sw.Elapsed}");
        }
    }
}
