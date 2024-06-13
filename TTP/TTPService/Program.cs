using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TTPService.Logging;

namespace TTPService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ValidateArgs(args);

            using var cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await BuildHost(args)
                    .RunAsync(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // ignored, The operation was canceled.
            }
        }

        public static IHost BuildHost(string[] args)
        {
            return CreateHostBuilder(args).Build();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseLogging()
                .ConfigureAppConfiguration( (hostingContext, config) =>
                {
                    config.AddJsonFile("ExperimentIdentifiers.json", optional: true, reloadOnChange: true);
                    config.AddCommandLine(args);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<Startup>());

        private static void ValidateArgs(string[] args)
        {
            string pattern = "PythonPath=\"*"; // Regex pattern

            var regex = new Regex(pattern);
            if (args.Any(arg => regex.Match(arg).Success))
            {
                return;
            }

            throw new InvalidDataException("must have parameter of king 'PythonPath=\"python_exe_location\"'");
        }

    }
}
