using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TTPService
{
    public static class ApiBehaviorExtensions
    {
        public static void AddApiBehavior(this IServiceCollection services, IConfiguration configuration)
        {
            // The [ApiController] attribute in the controller adds "Automatic HTTP 400 Responses" to the MVC pipeline
            // which means that the custom filter and action will not be executed if ModelState is invalid.
            // this configuration disables the Automatic HTTP 400 Response
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
        }
    }
}
