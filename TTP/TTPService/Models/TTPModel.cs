using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PredictTestTimeWrapper;
using Trace.Api.Common.TP;
using Trace.Common;
using TTPService.Enums;
using TTPService.FunctionalExtensions;
using TTPService.Helpers;
using Result = CSharpFunctionalExtensions.Result;

namespace TTPService.Models
{
    public class TTPModel : ITtpModel
    {
        private readonly ILogger logger;
        private readonly IMapper mapper;
        private readonly ITraceParserHelper traceParserHelper;
        private readonly string pythonPath;

        public TTPModel(IMapper mapper, ILogger<TTPModel> logger, ITraceParserHelper traceParserHelper, string pythonPath)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.traceParserHelper = traceParserHelper;
            this.pythonPath = pythonPath;
        }

        public async Task<Result<double, ErrorResult>> PredictAsync(
            string stplPath,
            string tplPath,
            string partType,
            string processStep,
            ExperimentType experimentType)
        {
            var testProgram = this.traceParserHelper.ParseTP(stplPath, tplPath);

            var parametersDictionary = new Dictionary<string, string>
            {
                ["IsConcurrent"] = testProgram?.IsConcurrent.ToString(),
                ["Patterns_Count"] = testProgram?.Plists?.Where(p => p.Patterns != null)
                    .SelectMany(p => p.Patterns)?.Distinct()?.Count().ToString() ?? "0",
                ["Tests_Count"] =
                    (this.traceParserHelper.GetAllElement<TestInstance>(testProgram)?.Count()).ToString(),
                ["Mtt_Count"] =
                    this.traceParserHelper.GetAllElement<MttTestInstance>(testProgram)?.Count().ToString(),    // add mtt another 2 times (total of 3 times))
                ["ConcurrentFlows_Count"] = this.traceParserHelper.GetAllElement<ConcurrentFlow>(testProgram)?.Count().ToString(),
                ["Shmoo_tests_count"] = this.traceParserHelper.GetAllElement<TestInstance>(testProgram)?.Count(ti => ti.Name.Contains("shmoo", StringComparison.OrdinalIgnoreCase)).ToString(),
                ["PartType"] = partType,
                ["ProcessStep"] = processStep,
                ["ExperimentType"] = experimentType.ToString() // correlation / engineering / walkTheLot
            };

            // TODO: read pythonExePath from environment variable?;
            // TODO: inject TTPWrapper to constructor & add interface;
            var pythonExePath = pythonPath;
            var ttpWrapper = new PredictTestTimeWrapper.PredictTestTimeWrapper(pythonExePath);
            var prediction = ttpWrapper.Predict(parametersDictionary);

            // 1. Get all the record data from trace API - as we did when we trained our model
            //  1.1 Do we fill the data from the trace API or from our .csv? if from .csv we need to add column of TP ?
            // 2. Build the record with ALL the columns (dummies makes all possible enum values to columns, we have only one value)
            // 3. Fill the record with the data
            // 4. Call the .py and return the result
            return Result.Ok<double, ErrorResult>(prediction.TotalSeconds);
        }
    }
}