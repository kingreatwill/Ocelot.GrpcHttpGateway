using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Built.Grpcc.SwaggerGen
{
    public static class ServiceDescriptorExtensions
    {
        public static List<MethodDescriptor> MethodList(this ServiceDescriptor serviceDescriptor)
        {
            var result = new List<MethodDescriptor>();
            foreach (var srv in serviceDescriptor.Descriptor)
            {
                foreach (var srvMethod in srv.Value)
                {
                    result.Add(srvMethod.Value);
                }
            }
            return result;
        }
    }
}