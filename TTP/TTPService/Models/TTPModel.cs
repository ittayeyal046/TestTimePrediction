using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Trace.Api.Common.TP;
using TTPService.Enums;
using TTPService.FunctionalExtensions;
using TTPService.Helpers;
using Result = CSharpFunctionalExtensions.Result;

namespace TTPService.Models
{
    public class TTPModel : ITtpModel
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ITraceParserHelper _traceParserHelper;

        public TTPModel(IMapper mapper, ILogger<TTPModel> logger , ITraceParserHelper traceParserHelper)
        {
            _logger = logger;
            _mapper = mapper;
            _traceParserHelper = traceParserHelper;
        }

        public async Task<Result<int, ErrorResult>> Predict(
            string stplPath,
            string tplPath,
            string partType,
            string processStep,
            ExperimentType experimentType)
        {
            var testProgram = _traceParserHelper.ParseTP(stplPath, tplPath);

            var data = new Dictionary<string, string>
            {
                ["IsConcurrent"] = testProgram?.IsConcurrent.ToString(),
                ["Patterns_Count"] = testProgram?.Plists?.Where(p => p.Patterns != null)
                    .SelectMany(p => p.Patterns)?.Distinct()?.Count().ToString(),
                ["Tests_Count"] =
                    (_traceParserHelper.GetAllElement<TestInstance>(testProgram)?.Count()).ToString(),
                ["Mtt_Count"] =
                    _traceParserHelper.GetAllElement<MttTestInstance>(testProgram)?.Count().ToString(),    // add mtt another 2 times (total of 3 times))
                ["ConcurrentFlows_Count"] = _traceParserHelper.GetAllElement<ConcurrentFlow>(testProgram)?.Count().ToString(),
                ["Shmoo_tests_count"] = _traceParserHelper.GetAllElement<TestInstance>(testProgram)?.Count(ti => ti.Name.Contains("shmoo", StringComparison.OrdinalIgnoreCase)).ToString(),
                ["ITuff_PartType_FromSpark"] = partType,
                ["ITuff_ProcessStep_FromSpark"] = processStep,
                ["ITuff_ExperimentType_FromSpark"] = experimentType.ToString(),  // correlation / engineering / walkTheLot
            };

            //TODO: ...
            //var ttpWrapper = new TTPWrapper();
            //...

            // 1. Get all the record data from trace API - as we did when we trained our model
            //  1.1 Do we fill the data from the trace API or from our .csv? if from .csv we need to add column of TP ?
            // 2. Build the record with ALL the columns (dummies makes all possible enum values to columns, we have only one value)
            // 3. Fill the record with the data
            // 4. Call the .py and return the result

            return Result.Ok<int, ErrorResult>(500);
        }
    }
}