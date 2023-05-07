using System.Diagnostics;
using Serilog;
using Trace.Api.Common.TP;
using Trace.Api.Common;
using Trace.Api.Common.Ituff;
using Trace.Api.Configuration;
using Trace.Api.Services.Common;
using Trace.Api.Services.TestProgramParser;
using Trace.Api.Services.TestProgramParser.Interfaces;
using Trace.Api.Services.TestResults.ItuffIndex;
using Trace.Api.Common.BinSwitch.FlowDiagram;
using Trace.Api.Common.Helpers;
using Trace.Api.Services.BinSwitch;
using Trace.Api.Services.BinSwitch.Interfaces;
using Trace.Api.Services.Cache;
using Torch.CodeAnalysis.Workspace.Services.Cache;
using Trace.Api.Services.TestResults.TestTime;
using Serilog.Core;

namespace TestTimePrediction;

public class TraceParser
{
    private readonly ILogger logger;

    public TraceParser(ILogger logger)
    {
        this.logger = logger;
    }

    public TestProgram GetTestProgram(IDriveMapping driveMapping, string stplPath, string tplPath)
    {
        logger.Information($"""
                          Start parsing testProgram:
                          stpl: {stplPath}
                          tpl: {tplPath}
                          """);
        Stopwatch sw = new Stopwatch();
        sw.Start();

        // now we need an instance to a test program parser
        // first we create the factory and ask it for a parser for the relevant test program type            
        var parserFactory = new TestProgramParserFactory(driveMapping);

        TestProgram testProgram = null;
        try
        {
            // parse the TP & ask for Plists    
            testProgram = parserFactory.ParseTestProgram(stplPath, tplPath);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to parse TestProgram from path {tplPath}, ex={ex}");
        }
        finally
        {
            sw.Stop();
            logger.Information($"End TestProgram parsing in {sw.Elapsed}");
        }
        
        return testProgram;
    }

    public IEnumerable<ClassItuffDefinition> GetClassITuffDefinitions()
    {
        logger.Information("Start GetClassITuffDefinitions");
        Stopwatch sw = new Stopwatch();
        sw.Start();

        // object containing IDC drives map
        var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);
        var fileService = new PassThroughFileService(driveMapping);

        IEnumerable<ClassItuffDefinition> ituffDefinitionList;

        // use that object to search for jobs. We will look for CLASS jobs in IDC
        using (var ituffIndexManager = new ItuffIndexManager(fileService))
        {
            // for the sample take the first ituff
            
            ituffDefinitionList = ituffIndexManager
                .GetAllItuffDefinitions()
                .Cast<ClassItuffDefinition>()
                .Where(i => i.Errors.IsNullOrEmpty()); // last 3 months
        }

        sw.Stop();
        logger.Information($"End RunTestInstances parsing testInstances in {sw.Elapsed}");

        return ituffDefinitionList;
    }

    /*
    public async Task<IEnumerable<TestInstance>> GetRunTestInstances(ItuffDefinition ituffDefinition)
    {
        logger.Information("Start GetRunTestInstances");
        Stopwatch sw = new Stopwatch();
        sw.Start();

        var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);
        var fileService = new PassThroughFileService(driveMapping);

        // that factory will prepare the session object containing all relevant data
        var sessionFactory = new SessionFactory(fileService);

        // start creating the session. That operation is async - for our demo we'll keep it simple and just wait for it to finish
        using (var session = sessionFactory.CreateSession(ituffDefinition))
        {
            logger.Information($"\nLoading data for: {ituffDefinition.Name} ...");

            // wait for the data to load
            await session.SessionStartup;

            //var visualId = session.Units.First().VisualId;

            // get instances that has runtime data, meaning at least one unit passed through it
            var runTestInstances =
                session.TestProgram.MainFlow.DeepSelect()
                    .OfType<TestInstance>()
                    .Where(
                        i =>
                            i.GetRuntimeModel<TpRuntimeModel>() != null);

            sw.Stop();
            logger.Information($"End GetRunTestInstances in {sw.Elapsed}");

            return runTestInstances;
        }
    }
    */

    public IEnumerable<UnitTestResult> GetUnitTestResults(TestInstance testInstance)
    {
        return testInstance.GetTestResults();
    }

    public IEnumerable<(string Key, (bool isPassed, TimeSpan TotalUnitRunTime))> CalcTestTimeForUnits(IDriveMapping driveMapping, ClassItuffDefinition ituffDefinition)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        logger.Information($"Start CalcTestTimeForUnits");

        var fileService = new PassThroughFileService(driveMapping);
        var sessionCreator = new SessionCreator(fileService);

        using (var session = sessionCreator.CreateSession(ituffDefinition))
        {
            logger.Information($"Loading data for: {ituffDefinition.Name} ...");

            // wait for the data to load
            session.SessionStartup.Wait();

            var testTimeDataCreator = new TestTimeDataCreator(null);

            // run the test time calculation based on the test time ituff tokens
            var testTimeData = testTimeDataCreator.CalculateTestTime(session, null, CancellationToken.None);

            // we have test time data per selected lot
            var lotTestTimeData = testTimeData.FirstOrDefault();

            if (lotTestTimeData == null)
            {
                logger.Error("Failed to create test time data in that lot");
                return new []{("NA", (false, TimeSpan.Zero))};
            }

            var unitsIdIsPassed = lotTestTimeData.UnitsExtraData
                .GroupBy(ur => ur.UnitId)
                .Select(g => (g.Key, g.Any(ui => ui.IsPassed))).ToDictionary(i => i.Key, i => i.Item2);

            var result = lotTestTimeData.TestInstancesRawData
                .SelectMany(ti => ti.UnitsResult)
                .GroupBy(ur => ur.UnitId)
                .Select(uig => (uig.Key, (unitsIdIsPassed[uig.Key], TimeSpan.FromMilliseconds(uig.Sum(ui => ui.Result)))));

            sw.Stop();
            logger.Information($"End CalcTestTimeForUnits, took {sw.Elapsed}");
            return result;
        }
    }
}
