using Built.Grpcc.Utils;
using Google.Protobuf.Reflection;
using Grpc.Core;
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
    public class ProtoPluginModel
    {
        public string FileName { get; set; }

        public string DllFileMd5 { get; set; }

        public string ProtoFileMd5 { get; set; }

        public string XmlFileMd5 { get; set; }
    }

    public class GrpcProtoFactory
    {
        private readonly ILogger logger;
        private readonly GrpcPluginFactory pluginFactory;
        private readonly CodeGenerater codeGenerater;
        private readonly CodeBuilder codeBuilder;

        public GrpcProtoFactory(GrpcPluginFactory pluginFactory, CodeGenerater codeGenerater, CodeBuilder codeBuilder, ILogger<GrpcProtoFactory> logger)
        {
            this.logger = logger;
            this.pluginFactory = pluginFactory;
            this.codeGenerater = codeGenerater;
            this.codeBuilder = codeBuilder;
        }

        public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public readonly string PluginPath = Path.Combine(BaseDirectory, "plugins");
        public readonly string ProtoPath = Path.Combine(BaseDirectory, "protos");

        // proto文件队列;
        public ProducerConsumer<string> ProtoQueue = new ProducerConsumer<string>(protoFileName =>
        {
            logger.LogDebug("出队:" + protoFileName);
            try
            {
                if (codeGenerater.Generate(BaseDirectory, protoFileName))
                {
                    InnerLogger.Log(LoggerLevel.Debug, "生成成功:" + protoFileName);
                    var name = Path.GetFileNameWithoutExtension(protoFileName);
                    var csharp_out = Path.Combine(BaseDirectory, $"plugins/.{name}");
                    if (CodeBuild.Build(csharp_out, name))
                    {
                        InnerLogger.Log(LoggerLevel.Debug, "Build成功:" + protoFileName);
                        var dllPath = Path.Combine(csharp_out, $"{name}.dll");
                        var xmlDocPath = Path.Combine(csharp_out, $"{name}.xml");
                        //生成plugin.yml
                        var serializer = new SerializerBuilder().Build();
                        var yaml = serializer.Serialize(new ProtoPluginModel
                        {
                            DllFileMD5 = dllPath.GetMD5(),
                            FileName = name,
                            ProtoFileMD5 = protoFileName.GetMD5(),
                            XmlFileMD5 = xmlDocPath.GetMD5()
                        });
                        File.WriteAllText(Path.Combine(csharp_out, "plugin.yml"), yaml);
                        DllQueue.Enqueue(dllPath);
                    }
                    else
                    {
                        InnerLogger.Log(LoggerLevel.Debug, "Build失败:" + protoFileName);
                    }
                }
                else
                {
                    InnerLogger.Log(LoggerLevel.Debug, "生成失败:" + protoFileName);
                }
            }
            catch (Exception er)
            {
                InnerLogger.Log(LoggerLevel.Debug, "出队:" + er.StackTrace);
            }
        });

        public Task InitAsync()
        {
            if (!Directory.Exists(PluginPath)) Directory.CreateDirectory(PluginPath);
            if (!Directory.Exists(ProtoPath)) Directory.CreateDirectory(ProtoPath);
            return Task.Run(() =>
            {
                var protoFiles = Directory.GetFiles(ProtoPath, "*.proto");
                foreach (var file in protoFiles)
                {
                    var NeedGenerate = true;
                    var GenerateDllPath = string.Empty;
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var csharp_out = Path.Combine(BaseDirectory, $"plugins/.{fileName}");
                    if (Directory.Exists(csharp_out))
                    {
                        var pluginYml = Path.Combine(csharp_out, $"plugin.yml");
                        GenerateDllPath = Path.Combine(csharp_out, $"{fileName}.dll");
                        var xmlDocPath = Path.Combine(csharp_out, $"{fileName}.xml");
                        if (File.Exists(pluginYml) && File.Exists(GenerateDllPath) && File.Exists(xmlDocPath))
                        {
                            var setting = new ProtoPluginModel();
                            var dic = new Dictionary<string, string>();

                            #region 文件读取

                            using (FileStream fs = new FileStream(pluginYml, FileMode.Open, FileAccess.Read))
                            {
                                using (StreamReader read = new StreamReader(fs, Encoding.Default))
                                {
                                    string strReadline;
                                    while ((strReadline = read.ReadLine()) != null)
                                    {
                                        var kv = strReadline.Split(':');
                                        if (kv.Length == 2)
                                        {
                                            dic.Add(kv[0], kv[1]);
                                        }
                                    }
                                }

                                dic.TryGetValue("FileName", out string fName);
                                setting.FileName = fName;
                                dic.TryGetValue("DllFileMd5", out string dName);
                                setting.DllFileMd5 = dName;
                                dic.TryGetValue("ProtoFileMd5", out string pName);
                                setting.ProtoFileMd5 = pName;
                                dic.TryGetValue("XmlFileMd5", out string xName);
                                setting.XmlFileMd5 = xName;
                            }

                            #endregion 文件读取

                            var protoMd5 = file.FileMd5();
                            var dllMd5 = GenerateDllPath.FileMd5();
                            var xmlMd5 = xmlDocPath.FileMd5();
                            if (setting.ProtoFileMd5 == protoMd5 && setting.DllFileMd5 == dllMd5 && setting.XmlFileMd5 == xmlMd5)
                            {
                                NeedGenerate = false;
                            }
                        }
                    }
                    if (NeedGenerate)
                    {
                        ProtoQueue.Enqueue(file);
                    }
                    else
                    {
                        pluginFactory.LoadAsync(GenerateDllPath);
                    }
                }
            });
        }

        public Task LoadAsync(string fileFullPath)
        {
            logger.LogDebug($"LoadAsync[{fileFullPath}]");
            return Task.Run(() =>
            {
                logger.LogDebug($"Run Start[{fileFullPath}]");

                logger.LogDebug($"Run End[{fileFullPath}]");
            });
        }
    }
}