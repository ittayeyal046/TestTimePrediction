using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace TTPService.Helpers
{
    public class HttpContextTokenFetcher : IHttpContextTokenFetcher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextTokenFetcher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<string>> GetToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return Result.Fail<string>("Failed to retrieve access token from http context, http context is null");
            }

            var accessToken = await httpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Result.Fail<string>("Failed to retrieve access token from current http context");
            }

            return Result.Ok(accessToken);
        }
    }
}