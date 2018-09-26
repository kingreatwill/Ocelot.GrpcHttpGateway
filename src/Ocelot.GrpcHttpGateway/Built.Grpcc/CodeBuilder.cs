using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using grpc = global::Grpc.Core;

namespace Built.Grpcc
{
    public class CodeBuilder
    {
        private ILogger logger;

        public CodeBuilder(ILogger<CodeBuilder> logger)
        {
            // 注册编码;
            //System.Text.Encoding.CodePages GB2312
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);//Encoding.GetEncoding("GB2312")
            this.logger = logger;
        }

        // public static ILogger logger = ServiceLocator.GetService<ILoggerFactory>().CreateLogger(nameof(Build));

        public bool Build(string csPath, string assemblyName)
        {
            var dllFiles = Directory.GetFiles(csPath, "*.cs");
            if (dllFiles.Length == 0) return false;
            List<SyntaxTree> trees = new List<SyntaxTree>();
            foreach (var file in dllFiles)
            {
                var csStr = File.ReadAllText(file, encoding: Encoding.GetEncoding("GB2312"));
                trees.Add(CSharpSyntaxTree.ParseText(csStr, encoding: Encoding.UTF8));
            }
            var references2 = new[]{
                MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=0.0.0.0").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.IO, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Threading.Tasks, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Google.Protobuf.ProtoPreconditions).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(grpc.Channel).Assembly.Location),
            };
            var options = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release);
            var compilation = CSharpCompilation.Create(assemblyName, trees, references2, options);
            var result2 = compilation.Emit(Path.Combine(csPath, $"{assemblyName}.dll"), xmlDocumentationPath: Path.Combine(csPath, $"{assemblyName}.xml"));
            logger.Log(
                result2.Success ? LogLevel.Debug : LogLevel.Error,
                string.Join(",", result2.Diagnostics.Select(d => string.Format("[{0}]:{1}({2})", d.Id, d.GetMessage(), d.Location.GetLineSpan().StartLinePosition)))
                );
            Thread.Sleep(100);
            return result2.Success;
        }
    }
}