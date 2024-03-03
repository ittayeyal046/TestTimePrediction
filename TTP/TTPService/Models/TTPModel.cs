using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            // 1. Get all the record data from trace API - as we did when we trained our model
            //  1.1 Do we fill the data from the trace API or from our .csv? if from .csv we need to add column of TP ?
            // 2. Build the record with ALL the columns (dummies makes all possible enum values to columns, we have only one value)
            // 3. Fill the record with the data
            // 4. Call the .py and return the result

            // Path to the Python executable
            string pythonExe = @"C:\Python27\python.exe";

            // Path to the Python script you want to run
            string pythonScript = "PredictTestTime.py";

            // TODO: Fill the args with ALL the record data
            List<string> args = new List<string>();

            // Create a ProcessStartInfo object
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"{pythonScript} {string.Join(" ", args)}",
                RedirectStandardOutput = true
            };

            // Start the Python process
            using Process? process = Process.Start(psi);

            // Read the output of the Python process
            string? output = process?.StandardOutput.ReadToEnd();
            process?.WaitForExit();

            if (output != null)
            {
                var processOutput = JsonConvert.DeserializeObject<Dictionary<string, object>>(output);

                if (processOutput != null)
                {
                    Console.WriteLine($"Python output:");

                    foreach (var kvp in processOutput)
                    {
                        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                    }
                }
            }

            return Result.Ok<int, ErrorResult>(500);

            //var experimentGroupToCreate = _mapper.Map<ExperimentGroup>(experimentGroupForCreationDto);
            //var repositoryCreateResult = await _repository.AddExperimentGroup(experimentGroupToCreate);
            //if (repositoryCreateResult.IsFailure)
            //{
            //    _logger.LogError(
            //        "Failed to add experiment group to repository {@ExperimentGroup}. {Error}",
            //        experimentGroupForCreationDto,
            //        repositoryCreateResult.Error);
            //    return ResultGenerator.RepositoryError<PredictionRecordDto>();
            //}

            //var createdExperimentGroup = repositoryCreateResult.Value;
            //_logger.LogInformation(
            //    $"Experiment group with id: {createdExperimentGroup.Id} was successfully added to repository.");
            //var createExperimentGroupDto = _mapper.Map<ExperimentGroup, ExperimentGroupCreationDto>(createdExperimentGroup);
            //var submissionResult = await _submitter.SubmitExperimentGroup(createExperimentGroupDto);
            //if (submissionResult.IsFailure)
            //{
            //    _logger.LogCritical("Failed to submit new experiment group. {Error}. {@ExperimentGroup}", submissionResult.Error, createdExperimentGroup);

            //    // todo: currently in this case we are removing the new experiment group from the repository. in the future we will implement a retry mechanism
            //    var repositoryDeleteResult = await _repository.RemoveExperimentGroup(createdExperimentGroup.Id);
            //    if (repositoryDeleteResult.IsFailure)
            //    {
            //        _logger.LogError(
            //            "Failed to delete experiment group from repository. Experiment Group Id: {Id}. {Error}",
            //            createdExperimentGroup.Id,
            //            repositoryDeleteResult.Error);
            //        return ResultGenerator.RepositoryError<PredictionRecordDto>();
            //    }

            //    return ResultGenerator.QueueError<PredictionRecordDto>();
            //}

            //_logger.LogInformation(
            //    "Creation of new experiment group was successfully submitted. " +
            //    $"Experiment Group ID: {repositoryCreateResult.Value.Id}");

            //var updateSubmissionToQueueStateResult = await _repository.UpdateExperimentGroupSubmissionToQueueState(
            //    createdExperimentGroup.Id,
            //    true);

            //// todo: need to consider this case in the retry mechanism
            //if (updateSubmissionToQueueStateResult.IsFailure)
            //{
            //    _logger.LogError(
            //        "Failed to update submission to queue state of created experiment group in repository. {@ExperimentGroup} {Error}",
            //        createdExperimentGroup,
            //        updateSubmissionToQueueStateResult.Error);
            //    return ResultGenerator.RepositoryError<PredictionRecordDto>();
            //}

            //_logger.LogInformation(
            //    "successfully updated the submission to queue state of created experiment group with ID: " +
            //    $"{createdExperimentGroup.Id}");

            //var experimentGroupToReturn = _mapper.Map<PredictionRecordDto>(repositoryCreateResult.Value);
            //return Result.Ok<PredictionRecordDto, ErrorResult>(experimentGroupToReturn);
        }
    }
}