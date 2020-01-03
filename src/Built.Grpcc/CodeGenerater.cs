using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Built.Grpcc
{
    public class CodeGenerater
    {
        // public static ILogger logger = ServiceLocator.GetService<ILoggerFactory>().CreateLogger(nameof(Generate));
        private ILogger logger;

        public CodeGenerater(ILogger<CodeGenerater> logger)
        {
            this.logger = logger;
        }

        public bool Generate(string baseDirectory, string protoFile)
        {
            var architecture = RuntimeInformation.OSArchitecture.ToString().ToLower();// 系统架构,x86 x64
            var bin = string.Empty;
            var os = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                os = "windows";
                bin = ".exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                os = "linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                os = "macosx";
            }
            else
            {
                logger.LogError("该平台不支持grpctools.");
                return false;
            }
            var protocBinPath = Path.Combine(baseDirectory, $"tools/{os}_{architecture}/protoc{bin}");
            var pluginBinPath = Path.Combine(baseDirectory, $"tools/{os}_{architecture}/grpc_csharp_plugin{bin}");
            var csharp_out = Path.Combine(baseDirectory, $"plugins/.{Path.GetFileNameWithoutExtension(protoFile)}");
            // 创建文件夹
            if (!Directory.Exists(csharp_out)) Directory.CreateDirectory(csharp_out);
            var proto_path = Path.Combine(baseDirectory, "protos");
            var protoc_args = $" --proto_path={proto_path} --csharp_out {csharp_out} {Path.GetFileName(protoFile)} --grpc_out {csharp_out} --plugin=protoc-gen-grpc={pluginBinPath}";
            Console.WriteLine(protocBinPath + "     " + protoc_args);
            var psi = new ProcessStartInfo(protocBinPath, protoc_args)
            {
                RedirectStandardOutput = true
            };
            //启动
            using (var proc = System.Diagnostics.Process.Start(psi))
            {
                if (proc == null)
                {
                    logger.LogError($"Can not process:{psi}");
                    return false;
                }
                else
                {
                    var output = proc.StandardOutput.ReadToEnd();
                    logger.LogDebug($"Process output:{output}");
                }
                Thread.Sleep(100);
            }
            return true;
        }
    }
}