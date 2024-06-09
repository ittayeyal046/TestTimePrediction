using System.Diagnostics;
using Serilog;
using Trace.Api.Common;
using Trace.Api.Common.Helpers;
using Trace.Api.Common.Ituff;
using Trace.Api.Common.TP;
using Trace.Api.Configuration;
using Trace.Api.Services.BinSwitch;
using Trace.Api.Services.BinSwitch.Interfaces;
using Trace.Api.Services.Cache;
using Trace.Api.Services.Common;
using Trace.Api.Services.TestProgramParser;
using Trace.Api.Services.TestProgramParser.Interfaces;
using Trace.Api.Services.TestResults.ItuffIndex;
using Trace.Api.Services.TestResults.TestTime;

namespace PrepareTrainingData;

/// <summary>
/// The trace parser.
/// </summary>
public class TraceParser
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraceParser"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public TraceParser(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Gets the test program.
    /// </summary>
    /// <param name="driveMapping">The drive mapping.</param>
    /// <param name="stplPath">The stpl path.</param>
    /// <param name="tplPath">The tpl path.</param>
    /// <returns>A TestProgram.</returns>
    public TestProgram GetTestProgram(IDriveMapping driveMapping, string stplPath, string tplPath)
    {
        this.logger.Information($"""
                          Start parsing testProgram:
                          stpl: {stplPath}
                          tpl: {tplPath}
                          """);
        Stopwatch sw = new Stopwatch();
        sw.Start();

        // now we need an instance to a test program parser
        // first we create the factory and ask it for a parser for the relevant test program type            
        var parserFactory = new TestProgramParserFactory(driveMapping);
        var parserFlag = /*EnumTpParserFlag.PgmRules | EnumTpParserFlag.UserVarsAndTosRules | EnumTpParserFlag.LocationSets | EnumTpParserFlag.Ph*/EnumTpParserFlag.Basic;

        TestProgram testProgram = null;
        try
        {
            // parse the TP & ask for Plists    
            testProgram = parserFactory.ParseTestProgram(stplPath, tplPath, parserFlag);
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

    /// <summary>
    /// Gets the class i tuff definitions.
    /// </summary>
    /// <returns>A list of ClassItuffDefinitions.</returns>
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
            ituffDefinitionList = ituffIndexManager
                .GetAllItuffDefinitions()
                .Cast<ClassItuffDefinition>()
                .Where(i => i.Errors.IsNullOrEmpty()); // last 3 months
        }

        sw.Stop();
        logger.Information($"End RunTestInstances parsing testInstances in {sw.Elapsed}");

        return ituffDefinitionList;
    }

    /// <summary>
    /// Calcs the test time for units async.
    /// </summary>
    /// <param name="driveMapping">The drive mapping.</param>
    /// <param name="ituffDefinition">The ituff definition.</param>
    /// <returns>A Task.</returns>
    public async Task<IEnumerable<(string Key, (bool IsPassed, TimeSpan TotalUnitRunTime))>> CalcTestTimeForUnitsAsync(IDriveMapping driveMapping, ClassItuffDefinition ituffDefinition)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        logger.Information($"Start CalcTestTimeForUnits");

        var fileService = new PassThroughFileService(driveMapping);
        var sessionCreator = new SessionCreator(fileService);

        using var session = sessionCreator.CreateSession(ituffDefinition);

        logger.Information($"Loading data for: {ituffDefinition.Name} ...");

        // wait for the data to load
        await session.SessionStartup;

        var testTimeDataCreator = new TestTimeDataCreator(null);

        // run the test time calculation based on the test time ituff tokens
        var testTimeData = testTimeDataCreator.CalculateTestTime(session, null, CancellationToken.None);

        // we have test time data per selected lot
        var lotTestTimeData = testTimeData.FirstOrDefault();

        if (lotTestTimeData == null)
        {
            logger.Error("Failed to create test time data in that lot");
            return new[] { ("NA", (false, TimeSpan.Zero)) };
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
