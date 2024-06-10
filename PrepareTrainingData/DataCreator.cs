using PrepareTrainingData;
using Trace.Api.Common.Ituff;
using Trace.Api.Common.TP;
using Trace.Api.Services.Common;

public class DataCreator : IDataCreator
{
    public async Task<IList<Dictionary<string, string>>> FillRecordsAsync(IDriveMapping driveMapping, TraceParser traceParser, ClassItuffDefinition ituffDefinition, TestProgram testProgram)
    {
        var list = new List<Dictionary<string, string>>();
        IEnumerable<(string Key, (bool IsPassed, TimeSpan TotalUnitRunTime))> testTimePerUnit;

        try
        {
            testTimePerUnit = await traceParser.CalcTestTimeForUnitsAsync(driveMapping, ituffDefinition);
        }
        catch
        {
            return list;
        }

        foreach (var testTime4SingleUnit in testTimePerUnit)
        {
            var singleRecord = new Dictionary<string, string>
            {
                ["TestProgram_Name_NA"] = testProgram?.Name,
                ["Family"] = testProgram?.Family,
                ["IsConcurrent"] = testProgram?.IsConcurrent.ToString(),
                ["Patterns_Count"] = testProgram?.Plists?.Where(p => p.Patterns != null)
                    .SelectMany(p => p.Patterns)?.Distinct()?.Count().ToString(),
                ["Tests_Count"] = 
                    (GetAllElement<TestInstance>(testProgram)?.Count()).ToString(),
                ["Mtt_Count"] = 
                    GetAllElement<MttTestInstance>(testProgram)?.Count().ToString(),    // add mtt another 2 times (total of 3 times))
                ["ConcurrentFlows_Count"] = GetAllElement<ConcurrentFlow>(testProgram)?.Count().ToString(),
                ["Shmoo_tests_count"] = GetAllElement<TestInstance>(testProgram)?.Count(ti => ti.Name.Contains("shmoo", StringComparison.OrdinalIgnoreCase)).ToString(),

                ["ITuff_Temperature_NA"] = ituffDefinition.Temperature,
                ["ITuff_SubmitterFullName_NA"] = ituffDefinition.SubmitterFullName,
                ["ITuff_Lot_NA"] = ituffDefinition.Lot,
                ["ituff_EndDate_NA"] = ituffDefinition.EndDate.ToString(),

                ["ITuff_PartType_FromSpark"] = ituffDefinition.PartType,
                ["ITuff_BomGroup_FromSpark"] = ituffDefinition.BomGroup,
                ["ITuff_ProcessStep_FromSpark"] = ituffDefinition.GetProcessStep(),
                ["ITuff_ExperimentType_FromSpark"] = ituffDefinition.ExperimentType,  // correlation / engineering / walkTheLot

                ["ITuff_PerUnit_IsPassed_Target_NA"] = testTime4SingleUnit.Item2.IsPassed.ToString(),
                ["ITuff_PerUnit_testTimeInMS_Target"] = testTime4SingleUnit.Item2.TotalUnitRunTime.TotalSeconds.ToString()
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