
using System.Diagnostics;
using TestTimePrediction;
using Trace.Api.Common;
using Trace.Api.Common.Helpers;
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
            Console.WriteLine("Main starting...");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // object containing IDC network drives map
            var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);

            // get path to stpl and tpl file of latest executed TP
            GetNewestTestProgramPathFromITuff(driveMapping, out string stplPath, out string tplPath);

            var traceParser = new TraceParser();
            TestProgram testProgram = traceParser.GetTestProgram(driveMapping, stplPath, tplPath);

            var allItuffDefinitions = traceParser.GetClassITuffDefinitions();
            var lastItuffDefinition = Enumerable.MaxBy(allItuffDefinitions, i => i.EndDate);
            //var singleItuffTestInstances = (await traceParser.GetRunTestInstances(ituffDefinition)).ToArray();

            IDataCreator dataCreator = new DataCreator();
            var records = dataCreator.FillRecords(driveMapping, traceParser, lastItuffDefinition, testProgram);

            var csv = new Csv();
            csv.Write(@"C:\Temp\TestPredictionResults.csv", records);

            sw.Stop();
            Console.WriteLine($"Program run took {sw.Elapsed}");
        }

        private static void GetNewestTestProgramPathFromITuff(IDriveMapping driveMapping, out string stplPath, out string tplPath)
        {
            // prepare index manager to retrieve jobs info
            using (var ituffIndexManager = new ItuffIndexManager(driveMapping))
            {
                // get latest job info
                var allItuffDefinition = ituffIndexManager
                    .GetAllItuffDefinitions()
                    .Where(i => i.Errors.IsNullOrEmpty());
                //.MaxBy(i => i.EndDate);
                var ituffDefinition = LinqExtensions.MaxBy(allItuffDefinition, i => i.EndDate);

                // depending on EVG version which was used for that job, it may be null
                stplPath = ituffDefinition.StplPath;
                tplPath = ituffDefinition.TplPath;
            }
        }
    }
}
