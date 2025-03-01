using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
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
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(options.ApplicationUrl);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<ConsoleHostedService>();
                });

        public class ConsoleHostedService : IHostedService
        {
            private readonly Options _options;

            public ConsoleHostedService(Options options)
            {
                _options = options;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                string localIp = GetLocalIPAddress();
                var ip = _options.ApplicationUrl.Split(':')[2];
                Log.Logger.Information($"TTP Server is Running on {_options.ApplicationUrl}");
                Log.Logger.Information($"Access TTP Service from another resource is available with url: {localIp}:{ip}");
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                Log.Logger.Information("TTP Server is stopping...");
                return Task.CompletedTask;
            }

            private string GetLocalIPAddress()
            {
                foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                        networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork) // IPv4
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
                return "127.0.0.1"; // Fallback if no network is found
            }
        }

        private static void HandleParseError(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error.ToString());
            }
        }
    }
}