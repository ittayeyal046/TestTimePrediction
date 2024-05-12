using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TTPService.Helpers;
using TTPService.Models;

namespace TTPService
{
    internal static class RegisterServices
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ITtpModel, TTPModel>();
            services.AddTransient<ITraceParserHelper, TraceParserHelper>();
            services.AddTransient<IHttpContextTokenFetcher, HttpContextTokenFetcher>();

            return services;
        }
    }
}