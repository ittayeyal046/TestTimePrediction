using System.Collections.Generic;
using Trace.Api.Common.TP;

namespace TTPService.Helpers;

public interface ITraceParserHelper
{
    TestProgram ParseTP(string stplPath, string tplPath);

    IEnumerable<T> GetAllElement<T>(TestProgram testProgram) where T : TpItemBase;
}