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

        protected virtual void GetGrpcDescript(Type[] types)
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
                    if (Descriptor.ContainsKey(svr.Name.ToUpper()))
                        continue;
                    if (Descriptor.TryAdd(svr.Name.ToUpper(), new ConcurrentDictionary<string, MethodDescriptor>()))
                        foreach (var method in svr.Methods)
                        {
                            Descriptor[svr.Name.ToUpper()].TryAdd(method.Name.ToUpper(), method);
                        }
                }
            }
        }
    }
}