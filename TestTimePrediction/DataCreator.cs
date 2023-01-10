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
                ["testProgram_Name"] = testProgram?.Name,
                ["testProgram_BomGroup"] = testProgram?.BomGroup,
                ["testProgram_Family"] = testProgram?.Family,
                ["testProgram_SubFamily"] = testProgram?.SubFamily,
                ["testProgram_FlowsCollection_Count"] = testProgram?.FlowsCollection.Count.ToString(),
                ["testProgram_IsConcurrent"] = testProgram?.IsConcurrent.ToString(),
                ["testProgram_Patterns_Count"] = testProgram?.Plists?.Where(p => p.Patterns != null)
                    .SelectMany(p => p.Patterns)?.Distinct()?.Count().ToString(),

                ["ituff_unit_testTimeInMS"] = testTime.Item2.Milliseconds.ToString()
            };

            list.Add(singleRecord);
        }



        /*
        singleRecord["ituff_EndDate"] = ituffDefinition?.EndDate.ToString();
        singleRecord["ituff_Lot"] = ituffDefinition?.Lot;
        singleRecord["ituff_ExperimentType"] = ituffDefinition?.ExperimentType;
        singleRecord["ituff_MaterialType"] = ituffDefinition?.MaterialType.ToString();

        singleRecord["ituff_RunTest_Count"] = singleItuffTestInstances?.Count().ToString();
        singleRecord["ituff_HotCold"] = ituffDefinition?.CurrentProcessStep;
        singleRecord["ituff_shmooTests_count"] = singleItuffTestInstances?.Count(ti => ti.FullName.ToLower().Contains("shmoo")).ToString();
        */


        /*
        // write  templates from all ITUFFs

        var templatesKeyToAmount = singleItuffTestInstances.GroupBy(ti => ti.TestTemplate.TemplateType)
            .Select(g => (g.Key, g.Count()));

        foreach (var group in templatesKeyToAmount)
        {
            singleRecord[group.Key] = group.;
        }
        
        testInstance_FullName = testInstance.FullName,
        testInstance_TemplateName = testInstance.TestTemplate.TemplateName,
        testInstance_TemplateType = testInstance.TestTemplate.TemplateType.ToString(),
        testInstance_IsEdcModeOn = testInstance.IsEdcModeOn.ToString(),
        testInstance_SpeedFlow_Name = testInstance.SpeedFlow?.Name ?? "-",
        testInstance_SpeedFlow_Instances_Count = testInstance.SpeedFlow?.Instances.Count.ToString() ?? "-" });
        
        records.Add("testInstance.SpeedFlow.Instances.Count", unitTest.);
        */

        return list;
    }
}