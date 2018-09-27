using Built.Grpcc.Utils;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Built.Grpcc
{
    public class ServiceDescriptor
    {
        public ConcurrentDictionary<string, ConcurrentDictionary<string, MethodDescriptor>> Descriptor { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, MethodDescriptor>>();

        public virtual void AddGrpcDescript(Type[] types)
        {
            var fileTypes = types.Where(type => type.Name.EndsWith("Reflection"));
            foreach (var type in fileTypes)
            {
                BindingFlags flag = BindingFlags.Static | BindingFlags.Public;
                var property = type.GetProperties(flag).Where(t => t.Name == "Descriptor").FirstOrDefault();
                if (property is null)
                    continue;
                var fileDescriptor = property.GetValue(null) as FileDescriptor;
                if (fileDescriptor is null)
                    continue;
                foreach (var svr in fileDescriptor.Services)
                {
                    var srvName = svr.FullName.ToUpper();
                    var methodDic = new ConcurrentDictionary<string, MethodDescriptor>();
                    foreach (var method in svr.Methods)
                    {
                        methodDic.TryAdd(method.Name.ToUpper(), method);
                    }
                    Descriptor.AddOrUpdate(srvName, methodDic);

                    //if (Descriptor.ContainsKey(srvName))
                    //    continue;
                    //if (Descriptor.TryAdd(srvName, new ConcurrentDictionary<string, MethodDescriptor>()))
                    //    foreach (var method in svr.Methods)
                    //    {
                    //        Descriptor[srvName].TryAdd(method.Name.ToUpper(), method);
                    //    }
                }
            }
        }
    }
}