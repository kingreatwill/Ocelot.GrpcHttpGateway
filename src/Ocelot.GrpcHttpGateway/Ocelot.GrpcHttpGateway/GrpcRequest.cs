using Google.Protobuf.Reflection;
using System.Collections.Generic;

namespace Ocelot.GrpcHttpGateway
{
    public class GrpcRequest
    {
        public MethodDescriptor GrpcMethod { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public object RequestMessage { get; set; }
    }
}