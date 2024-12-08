using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PredictTestTimeWrapper;
using Trace.Api.Common.TP;
using TTPService.Configuration;
using TTPService.Enums;
using TTPService.FunctionalExtensions;
using TTPService.Helpers;
using Result = CSharpFunctionalExtensions.Result;

namespace TTPService.Models
{
    public class TTPModel : ITTPModel
    {
        private readonly ILogger logger;
        private readonly ITraceParserHelper traceParserHelper;
        private readonly IPythonPathProvider pythonPathProvider;

        public TTPModel(ILogger<TTPModel> logger, ITraceParserHelper traceParserHelper, IPythonPathProvider pythonPathProvider)
        {
            this.logger = logger;
            this.traceParserHelper = traceParserHelper;
            this.pythonPathProvider = pythonPathProvider;
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

            // TODO: inject TTPWrapper to constructor & add interface;
            var pythonExePath = pythonPathProvider.PythonPath;
            IPredictTestTimeWrapper ttpWrapper = new PredictTestTimeWrapper.PredictTestTimeWrapper(pythonExePath);
            var prediction = await ttpWrapper.Predict(parametersDictionary);
            string parameters = string.Join(",", parametersDictionary.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            logger.LogInformation($"Predicated test time is {prediction.TotalSeconds} seconds for parameters {{{parameters}}}");

            // 1. Get all the record data from trace API - as we did when we trained our model
            //  1.1 Do we fill the data from the trace API or from our .csv? if from .csv we need to add column of TP ?
            // 2. Build the record with ALL the columns (dummies makes all possible enum values to columns, we have only one value)
            // 3. Fill the record with the data
            // 4. Call the .py and return the result
            return Result.Ok<double, ErrorResult>(prediction.TotalSeconds);
        }
    }
}