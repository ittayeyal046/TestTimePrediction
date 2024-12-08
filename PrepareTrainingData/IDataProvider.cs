using PrepareTrainingData;
using Trace.Api.Common.Ituff;
using Trace.Api.Common.TP;
using Trace.Api.Services.Common;

public interface IDataProvider
{
    Task<IList<Dictionary<string, string>>> FillRecordsAsync(IDriveMapping driveMapping, ITuffServices ituffServices, ClassItuffDefinition ituffDefinition, TestProgram testProgram);
}