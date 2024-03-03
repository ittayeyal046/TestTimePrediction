using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TTPService.Enums;
using TTPService.FunctionalExtensions;

namespace TTPService.Models
{
    public interface ITtpModel
    {
        Task<Result<int, ErrorResult>> Predict(
            string tpPath,
            string partType,
            string bomGroup,
            string processStep,
            ExperimentType experimentType);
    }
}
