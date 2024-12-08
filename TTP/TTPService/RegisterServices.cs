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
            services.AddTransient<ITTPModel, TTPModel>();
            services.AddTransient<ITraceParserHelper, TraceParserHelper>();

            return services;
        }
    }
}