using Built.Grpcc.Utils;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Logging;
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
        private readonly string PluginPath = Path.Combine(BaseDirectory, "plugins");

        private readonly ILogger logger;
        private readonly ServiceDescriptor serviceDescriptor;

        public GrpcPluginFactory(ServiceDescriptor serviceDescriptor, ILogger<GrpcPluginFactory> logger)
        {
            this.logger = logger;
            this.serviceDescriptor = serviceDescriptor;
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

        public Task LoadAsync(string fileFullPath)
        {
            logger.LogDebug($"LoadAsync[{fileFullPath}]");
            return Task.Run(() =>
            {
                logger.LogDebug($"Run Start[{fileFullPath}]");
                byte[] assemblyBuf = File.ReadAllBytes(fileFullPath);
                var assembly = Assembly.Load(assemblyBuf);
                var types = assembly.GetTypes();
                serviceDescriptor.AddGrpcDescript(types);
                logger.LogDebug($"Run End[{fileFullPath}]");
            });
        }
    }
}