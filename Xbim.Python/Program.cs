using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using Xbim.Python.Services;

namespace Xbim.Python
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(hostConfig =>
                {
                    hostConfig.SetBasePath(Directory.GetCurrentDirectory());
                    hostConfig.AddJsonFile("settings.json", optional: true);
                    hostConfig.AddEnvironmentVariables(prefix: "XBIM_");
                    hostConfig.AddCommandLine(args);
                })
                .ConfigureLogging(logConfig =>{
                    var log = new LoggerConfiguration();
                    log.WriteTo.Console();
                    var logger = log.CreateLogger();

                    // Xbim logging and application logging share the same logger
                    Xbim.Common.XbimLogging.LoggerFactory.AddSerilog(logger);
                    logConfig.AddSerilog(logger);
                })
                .ConfigureServices(services =>
                {
                    // root application service
                    services.AddHostedService<XbimPythonService>();
                })
                .Build()
                .RunAsync();
        }
    }
}
