using Built.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly GrpcHttpGatewayConfiguration config;
        public ProducerConsumer<string> protoQueue;

        public GrpcProtoFactory(GrpcPluginFactory pluginFactory, IOptions<GrpcHttpGatewayConfiguration> config, CodeGenerater codeGenerater, CodeBuilder codeBuilder, ILogger<GrpcProtoFactory> logger)
        {
            this.logger = logger;
            this.pluginFactory = pluginFactory;
            this.codeGenerater = codeGenerater;
            this.codeBuilder = codeBuilder;
            this.config = config.Value;
            protoQueue = new ProducerConsumer<string>(protoFileName => LoadAsync(protoFileName).Wait());
            if (this.config.ProtoMonitor)
                MonitorStart();
        }

        private DirectoryMonitor monitor;
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string PluginPath = Path.Combine(BaseDirectory, "plugins");
        private readonly string ProtoPath = Path.Combine(BaseDirectory, "protos");

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
                                            dic.Add(kv[0].Trim(), kv[1].Trim());
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
                        protoQueue.Enqueue(file);
                    }
                    else
                    {
                        pluginFactory.LoadAsync(GenerateDllPath);
                    }
                }
            });
        }

        private Task LoadAsync(string fileFullPath)
        {
            logger.LogDebug($"LoadAsync[{fileFullPath}]");
            return Task.Run(() =>
            {
                logger.LogDebug($"Run Start[{fileFullPath}]");
                logger.LogDebug("出队:" + fileFullPath);
                try
                {
                    if (codeGenerater.Generate(BaseDirectory, fileFullPath))
                    {
                        logger.LogDebug("生成成功:" + fileFullPath);
                        var name = Path.GetFileNameWithoutExtension(fileFullPath);
                        var csharp_out = Path.Combine(BaseDirectory, $"plugins/.{name}");
                        if (codeBuilder.Build(csharp_out, name))
                        {
                            logger.LogDebug("Build成功:" + fileFullPath);
                            var dllPath = Path.Combine(csharp_out, $"{name}.dll");
                            var xmlDocPath = Path.Combine(csharp_out, $"{name}.xml");

                            var model = new ProtoPluginModel
                            {
                                DllFileMd5 = dllPath.FileMd5(),
                                FileName = name,
                                ProtoFileMd5 = fileFullPath.FileMd5(),
                                XmlFileMd5 = xmlDocPath.FileMd5()
                            };

                            #region 文件写入

                            using (FileStream fs = new FileStream(Path.Combine(csharp_out, "plugin.yml"), FileMode.Create, FileAccess.Write))
                            {
                                using (StreamWriter writer = new StreamWriter(fs, Encoding.Default))
                                {
                                    writer.WriteLine($"FileName: {model.FileName}");
                                    writer.WriteLine($"DllFileMd5: {model.DllFileMd5}");
                                    writer.WriteLine($"ProtoFileMd5: {model.ProtoFileMd5}");
                                    writer.WriteLine($"XmlFileMd5: {model.XmlFileMd5}");
                                    writer.Flush();
                                }
                            }

                            #endregion 文件写入

                            // dll加载;
                            pluginFactory.LoadAsync(dllPath).Wait();
                        }
                        else
                        {
                            logger.LogError("Build失败:" + fileFullPath);
                        }
                    }
                    else
                    {
                        logger.LogError("生成失败:" + fileFullPath);
                    }
                }
                catch (Exception er)
                {
                    logger.LogError($"出队:{fileFullPath}", er);
                }
                logger.LogDebug($"Run End[{fileFullPath}]");
            });
        }

        /// <summary>
        /// 开启监控
        /// </summary>
        private void MonitorStart()
        {
            if (!Directory.Exists(ProtoPath)) Directory.CreateDirectory(ProtoPath);
            monitor = new DirectoryMonitor(ProtoPath, "*.proto");
            monitor.Change += (string filePath) =>
            {
                protoQueue.Enqueue(filePath);
            };
            monitor.Start();
        }
    }
}