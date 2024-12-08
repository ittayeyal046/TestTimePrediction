using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TTPService.Logging;

namespace TTPService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var parsedOptions = new Options();
            var parsedArgs = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => parsedOptions = options)
                .WithNotParsed(HandleParseError);

            if (parsedArgs.Tag == ParserResultType.NotParsed)
            {
                return;
            }

            using var cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await BuildHost(args, parsedOptions)
                    .RunAsync(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // ignored, The operation was canceled.
            }
        }

        public static IHost BuildHost(string[] args, Options options)
        {
            return CreateHostBuilder(args, options).Build();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, Options options) =>
            Host.CreateDefaultBuilder(args)
                .UseLogging()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("ExperimentIdentifiers.json", optional: true, reloadOnChange: true);
                    config.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(options);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<Startup>());

        private static void HandleParseError(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error.ToString());
            }
        }
    }
}