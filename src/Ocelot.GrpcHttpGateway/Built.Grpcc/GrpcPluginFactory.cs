using Built.Grpcc.Utils;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Built.Grpcc
{
    public class GrpcPluginFactory
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string PluginPath = Path.Combine(BaseDirectory, "plugins");

        private DirectoryMonitor monitor;
        private readonly ILogger logger;

        private readonly ServiceDescriptor serviceDescriptor;
        private readonly GrpcHttpGatewayConfiguration config;

        public GrpcPluginFactory(ServiceDescriptor serviceDescriptor, IOptions<GrpcHttpGatewayConfiguration> config, ILogger<GrpcPluginFactory> logger)
        {
            this.logger = logger;
            this.config = config.Value;
            this.serviceDescriptor = serviceDescriptor;
            if (this.config.PluginMonitor)
                MonitorStart();
        }

        public Task InitAsync()
        {
            if (!Directory.Exists(PluginPath)) Directory.CreateDirectory(PluginPath);
            return Task.Run(() =>
            {
                // 不包含子目录;如果有自己的dll可以放在这里;
                var dllFiles = Directory.GetFiles(PluginPath, "*.dll");
                foreach (var file in dllFiles)
                {
                    LoadAsync(file);
                }
            });
        }

        /// <summary>
        /// 开启监控
        /// </summary>
        private void MonitorStart()
        {
            if (!Directory.Exists(PluginPath)) Directory.CreateDirectory(PluginPath);
            monitor = new DirectoryMonitor(PluginPath, "*.dll");
            monitor.Change += (string filePath) =>
            {
                LoadAsync(filePath);
            };
            monitor.Start();
        }

        public Task LoadAsync(string fileFullPath)
        {
            logger.LogDebug($"LoadAsync[{fileFullPath}]");
            return Task.Run(() =>
            {
                logger.LogDebug($"Run Start[{fileFullPath}]");
                byte[] assemblyBuf = File.ReadAllBytes(fileFullPath);
                var assembly = Assembly.Load(assemblyBuf);
                var types = assembly.GetTypes();
                serviceDescriptor.AddGrpcDescript(fileFullPath, types);
                logger.LogDebug($"Run End[{fileFullPath}]");
            });
        }
    }
}