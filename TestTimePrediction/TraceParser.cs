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

namespace TestTimePrediction;

internal class TraceParser
{
    public TestProgram GetTestProgram()
    {
        // object containing IDC network drives map
        var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);

        // gets some valid test program path
        string stplPath, tplPath;

        // get path to stpl and tpl file of latest executed TP
        GetTestProgramPath(driveMapping, out stplPath, out tplPath);

        // now we need an instance to a test program parser
        // first we create the factory and ask it for a parser for the relevant test program type            
        var parserFactory = new TestProgramParserFactory(driveMapping);

        // parse the TP & ask for Plists    
        TestProgram testProgram = parserFactory.ParseTestProgram(stplPath, tplPath, EnumTpParserFlag.PLists);

        return testProgram;
    }

    private static void GetTestProgramPath(IDriveMapping driveMapping, out string stplPath, out string tplPath)
    {
        // prepare index manager to retrieve jobs info
        using (var ituffIndexManager = new ItuffIndexManager(driveMapping))
        {
            // get latest job info
            var allItuffDefinition = ituffIndexManager
                .GetAllItuffDefinitions()
                .Where(i => i.Errors.IsNullOrEmpty());
            //.MaxBy(i => i.EndDate);
            var ituffDefinition= LinqExtensions.MaxBy(allItuffDefinition, i => i.EndDate);

            // depending on EVG version which was used for that job, it may be null
            stplPath = ituffDefinition.StplPath;
            tplPath = ituffDefinition.TplPath;
        }
    }

    public IEnumerable<TestInstance> GetTestInstances()
    {
        // object containing IDC drives map
        var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);
        var fileService = new PassThroughFileService(driveMapping);

        ClassItuffDefinition firstItuffDefinition;

        // use that object to search for jobs. We will look for CLASS jobs in IDC
        using (var ituffIndexManager = new ItuffIndexManager(fileService))
        {
            // for the sample take the first ituff
            firstItuffDefinition = (ClassItuffDefinition)ituffIndexManager
                .GetAllItuffDefinitions()
                .First(i => i.Errors.IsNullOrEmpty());
        }

        // that factory will prepare the session object containing all relevant data
        var sessionFactory = new SessionFactory(fileService);

        // start creating the session. That operation is async - for our demo we'll keep it simple and just wait for it to finish
        using (var session = sessionFactory.CreateSession(firstItuffDefinition))
        {
            Console.WriteLine($"Loading data for: {firstItuffDefinition.Name} ...");

            // wait for the data to load
            session.SessionStartup.Wait();

            var visualId = session.Units.First().VisualId;

            // get instances that has runtime data, meaning at least one unit passed through it
            var testInstances =
                session.TestProgram.MainFlow.DeepSelect()
                    .OfType<TestInstance>()
                    .Where(
                        i =>
                            i.GetRuntimeModel<TpRuntimeModel>() != null);

            return testInstances;
        }
    }

    public IEnumerable<UnitTestResult> GetUnitTestResults(TestInstance testInstance)
    {
        return testInstance.GetTestResults();
    }
}
