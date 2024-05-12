﻿using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TTPService.Enums;
using TTPService.FunctionalExtensions;

namespace TTPService.Models
{
    public interface ITtpModel
    {
        Task<Result<int, ErrorResult>> Predict(
            string stplPath,
            string tplPath,
            string partType,
            string processStep,
            ExperimentType experimentType);
    }
}
