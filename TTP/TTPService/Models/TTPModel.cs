using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PredictTestTimeWrapper;
using Serilog;
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
        private readonly Serilog.ILogger logger;
        private readonly ITraceParserHelper traceParserHelper;
        private readonly IPythonPathProvider pythonPathProvider;

        public TTPModel(ITraceParserHelper traceParserHelper, IPythonPathProvider pythonPathProvider)
        {
            logger = Log.Logger;
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
            logger.Information($"Starting to parse test program, stpl: {stplPath}, tpl: {tplPath}");
            var testProgram = traceParserHelper.ParseTP(stplPath, tplPath);
            var parametersDictionary = BuildParametersDictionary(partType, processStep, experimentType, testProgram);

            logger.Information("Test parameters: {@Parameters}", parametersDictionary);

            var pythonExePath = pythonPathProvider.PythonPath;
            IPredictTestTimeWrapper ttpWrapper = new PredictTestTimeWrapper.PredictTestTimeWrapper(pythonExePath);

            logger.Information("going to call ttpWrapper with Test parameters...");
            var prediction = await ttpWrapper.Predict(parametersDictionary);
            string parameters = string.Join(",", parametersDictionary.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            logger.Information($"Predicated test time is {prediction.TotalSeconds} seconds for parameters {{{parameters}}}");

            return Result.Success<double, ErrorResult>(prediction.TotalSeconds);
        }

        private Dictionary<string, string> BuildParametersDictionary(string partType, string processStep, ExperimentType experimentType, TestProgram testProgram)
        {
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

            return parametersDictionary;
        }
    }
}