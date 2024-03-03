using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TTPService.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureSection<RepositoryOptions>(configuration);
            services.ConfigureSection<SystemBaseUrlsOptions>(configuration);
            services.ConfigureSection<RetryOptions>(configuration);
            return services;
        }

        /// <summary>
        /// Gets a configuration sub-section which T generics name is equal to configuration. T generics ends with 'Options' will be removed.
        /// </summary>
        /// <example>FooOptions will be stripped from Options string and Foo sub-section will be searched.</example>
        /// <typeparam name="T">The type of the new Options instance.</typeparam>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <returns>The new instance of T if successful, default(T) otherwise.</returns>
        public static T GetSectionValue<T>(this IConfiguration configuration)
            where T : class
        {
            var sectionName = GetSectionName<T>();
            return configuration.GetSection(sectionName).Get<T>();
        }

        /// <summary>
        /// Registers a configuration instance which TOptions will bind against.
        /// </summary>
        /// <typeparam name="T">The type of options being configured.</typeparam>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add the services to.</param>
        /// <param name="configuration">The configuration being bound.</param>
        /// <returns>The Microsoft.Extensions.DependencyInjection.IServiceCollection so that additional calls can be chained.</returns>
        public static IServiceCollection ConfigureSection<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class
        {
            services.Configure<T>(configuration.GetSection<T>());

            return services;
        }

        private static IConfigurationSection GetSection<T>(this IConfiguration configuration)
            where T : class
        {
            var sectionName = GetSectionName<T>();
            return configuration.GetSection(sectionName);
        }

        private static string GetSectionName<T>()
            where T : class
        {
            var name = typeof(T).Name;
            var section = name.Replace("Options", string.Empty);

            return section;
        }
    }
}
