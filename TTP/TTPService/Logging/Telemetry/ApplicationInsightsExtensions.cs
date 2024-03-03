using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TTPService.Logging.Telemetry
{
    public static class ApplicationInsightsExtensions
    {
        private const string ApplicationInsights = "ApplicationInsights";
        private const string CloudRoleName = "CloudRoleName";

        public static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection(ApplicationInsights);
            var cloudRoleName = configurationSection[CloudRoleName];
            services.AddSingleton<ITelemetryInitializer>(provider => new CloudRoleNameTelemetryInitializer(cloudRoleName));
            services.AddTransient<ApplicationInsightsLoggingMiddleware>();

            return services;
        }
    }
}
