using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TTPService.Enums;
using TTPService.FunctionalExtensions;

namespace TTPService.Models
{
    public class TTPModel : ITtpModel
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public TTPModel(IMapper mapper, ILogger<TTPModel> logger)
        {
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<int, ErrorResult>> Predict(
            string tpPath,
            string partType,
            string bomGroup,
            string processStep,
            ExperimentType experimentType)
        {
            var parametersDictionary = new Dictionary<string, string>()
            {
                { "tpPath", tpPath },
                { "partType", partType },
                { "bomGroup", bomGroup },
                { "processStep", processStep },
                { "experimentType", experimentType.ToString() },
            };

            // 1. Get all the record data from trace API - as we did when we trained our model
            //  1.1 Do we fill the data from the trace API or from our .csv? if from .csv we need to add column of TP ?
            // 2. Build the record with ALL the columns (dummies makes all possible enum values to columns, we have only one value)
            // 3. Fill the record with the data
            // 4. Call the .py and return the result

            return Result.Ok<int, ErrorResult>(500);
        }
    }
}