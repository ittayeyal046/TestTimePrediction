using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace TTPService.Helpers
{
    public interface IHttpContextTokenFetcher
    {
        Task<Result<string>> GetToken();
    }
}