using Grpc.Core;
using Grpc.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Built.Grpc.Middleware
{
    /// <summary>
    /// MiddlewareContext
    /// </summary>
    public class MiddlewareContext
    {
        public IMethod Method { get; set; }

        /// <summary>
        /// Request object, null if streaming.
        /// </summary>
        public object Request { get; set; }

        /// <summary>
        /// Response object, null if streaming or on request path.
        /// </summary>
        public object Response { get; set; }

        public string Host { get; set; }

        public CallOptions Options { get; set; }

        /// <summary>
        /// Final handler of the RPC
        /// </summary>
        internal Func<Task> HandlerExecutor { get; set; }
    }
}