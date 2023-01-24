using TestProgramParsers.Ph;
using TestTimePrediction;
using Trace.Api.Common.Ituff;
using Trace.Api.Common.TP;
using Trace.Api.Services.BinSwitch;
using Trace.Api.Services.Common;
using Trace.Api.Services.TestResults.TestTime;

public class DataCreator : IDataCreator
{
    public IEnumerable<Dictionary<string, string>> FillRecords(IDriveMapping driveMapping, TraceParser traceParser,
        ClassItuffDefinition ituffDefinition, TestProgram testProgram)
    {
        var list = new List<Dictionary<string, string>>();

        var testTimePerUnit = traceParser.CalcTestTimeForUnits(driveMapping, ituffDefinition);

        foreach (var testTime in testTimePerUnit)
        {
            var singleRecord = new Dictionary<string, string>
            {
                ["Name"] = testProgram?.Name,
                ["BomGroup"] = testProgram?.BomGroup,
                ["Family"] = testProgram?.Family,
                ["SubFamily"] = testProgram?.SubFamily,
                //["Socket"] = testProgram?.,
                ["IsConcurrent"] = testProgram?.IsConcurrent.ToString(),
                ["Patterns_Count"] = testProgram?.Plists?.Where(p => p.Patterns != null)
                    .SelectMany(p => p.Patterns)?.Distinct()?.Count().ToString(),
                ["Tests_And_Mttx3_Count"] = 
                    (GetAllElement<TestInstance>(testProgram)?.Count() +                     // includes the mtt
                     GetAllElement<MttTestInstance>(testProgram)?.Count() * 2).ToString(),    // add mtt another 2 times (total of 3 times))
                ["ConcurrentFlows_Count"] = GetAllElement<ConcurrentFlow>(testProgram)?.Count().ToString(),
                ["Shmoo_tests_count"] = GetAllElement<TestInstance>(testProgram)?.Count(ti => ti.Name.Contains("shmoo", StringComparison.OrdinalIgnoreCase)).ToString(),


                ["ituff_EndDate"] = ituffDefinition.EndDate.ToString(),
                ["ituff_PerUnit_testTimeInMS"] = testTime.TotalUnitRunTime.Milliseconds.ToString()
            };

            list.Add(singleRecord);
        }

        return list;
    }

    private IEnumerable<T> GetAllElement<T>(TestProgram testProgram) where T : TpItemBase
    {
        return testProgram?.DeepSelect<T>(true);
    }
}