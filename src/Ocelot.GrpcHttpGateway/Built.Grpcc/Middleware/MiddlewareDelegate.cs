using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Built.Grpcc
{
    /// <summary>
    /// Middleware function for plugging into RPC pipeline.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public delegate Task PipelineDelagate(MiddlewareContext context);
}