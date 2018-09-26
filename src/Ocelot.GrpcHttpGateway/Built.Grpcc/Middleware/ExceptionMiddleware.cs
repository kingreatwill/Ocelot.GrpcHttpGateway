using System;
using System.Threading.Tasks;

namespace Built.Grpcc
{
    public class ExceptionMiddleware
    {
        private PipelineDelagate _next;

        public ExceptionMiddleware(PipelineDelagate next)
        {
            _next = next;
        }

        public async Task Invoke(MiddlewareContext context)
        {
            try
            {
                await _next(context);
            }
            catch //(Exception e)
            {
                //log it
                // context.Response.Status = new Grpc.Core.Status(Grpc.Core.StatusCode.Internal, "Ooops");
            }
        }
    }
}