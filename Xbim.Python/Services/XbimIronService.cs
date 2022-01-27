using IronPython.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace Xbim.Python.Services
{
    internal class XbimPythonService : IHostedService
    {
        private readonly IHostApplicationLifetime lifetime;
        private readonly IConfiguration configuration;
        private readonly ILogger<XbimPythonService> logger;
        private readonly ILoggerFactory loggerFactory;

        public XbimPythonService(IHostApplicationLifetime lifetime, IConfiguration configuration, ILogger<XbimPythonService> logger, ILoggerFactory loggerFactory)
        {
            this.lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loggerFactory = loggerFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            lifetime.ApplicationStarted.Register(OnStarted);
            lifetime.ApplicationStopping.Register(OnStopping);
            lifetime.ApplicationStopped.Register(OnStopped);

            var modelFile = configuration["model"];
            if (string.IsNullOrWhiteSpace(modelFile) || !File.Exists(modelFile))
            {
                logger.LogError("No IFC model file supplied");
                return Task.CompletedTask; ;
            }

            var scriptFile = configuration["script"];
            if (string.IsNullOrWhiteSpace(scriptFile) || !File.Exists(scriptFile))
            {
                logger.LogError("No Python script file supplied");
                return Task.CompletedTask; ;
            }

            // general catch
            try
            {
                var ipy = IronPython.Hosting.Python.CreateEngine();
                ipy.Runtime.IO.SetOutput(Console.OpenStandardOutput(), Console.OutputEncoding);

                // add script and module search paths
                var pyPaths = ipy.GetSearchPaths();
                var currentPath = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
                var scriptPath = new FileInfo(scriptFile).DirectoryName;
                pyPaths.Add(currentPath);
                pyPaths.Add(scriptPath);
                ipy.SetSearchPaths(pyPaths);

                

                var script = ipy.CreateScriptSourceFromFile(scriptFile);
                var loader = ipy.CreateScriptSourceFromString(@"
import clr

clr.AddReference('System.Core')
clr.AddReference('Xbim.Common')
clr.AddReference('Xbim.Ifc4')
clr.AddReference('Xbim.Ifc2x3')

import System
clr.ImportExtensions(System.Linq)
                "); 

                IfcStore.ModelProviderFactory.UseMemoryModelProvider();
                var log = loggerFactory.CreateLogger("Script");
                log.LogInformation("Script to be executed: '{script}'", scriptFile);
                using (var model = IfcStore.Open(modelFile))
                { 
                    // create a scope where the model is accessible
                    var scopeData = new Dictionary<string, object>
                    {
                        ["model"] = model,
                        ["instances"] = model.Instances,
                        ["create"] = new Create(model),
                        ["information"] = new Action<string>(msg => log.LogInformation(msg)),
                        ["error"] = new Action<string>(msg => log.LogError(msg)),
                        ["warning"] = new Action<string>(msg => log.LogWarning(msg)),
                    };
                    var pyScope = ipy.CreateScope(scopeData);

                    loader.Execute(pyScope);
                    script.Execute(pyScope);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "General failure: " + e.Message);
                throw;
            }
            finally
            { 
                lifetime.StopApplication();
            }

            return Task.CompletedTask; ;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask; ;
        }

        private void OnStarted()
        {
            // ...
        }

        private void OnStopping()
        {
            // ...
        }

        private void OnStopped()
        {
            // ...
        }
    }
}
