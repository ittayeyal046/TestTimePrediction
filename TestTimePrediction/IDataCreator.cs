using TestTimePrediction;
using Trace.Api.Common.Ituff;
using Trace.Api.Common.TP;
using Trace.Api.Services.Common;

public interface IDataCreator
{
    IEnumerable<Dictionary<string, string>> FillRecords(IDriveMapping driveMapping, TraceParser traceParser,
        ClassItuffDefinition ituffDefinition,
        TestProgram testProgram);
}