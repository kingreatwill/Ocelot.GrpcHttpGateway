using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Built.Grpcc
{
    public class LoggingMiddleware
    {
        private readonly PipelineDelagate _next;
        private readonly ILogger _logger;

        public LoggingMiddleware(PipelineDelagate next, ILogger<CodeBuilder> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(MiddlewareContext context)
        {
            _logger.LogInformation($"LoggingMiddleware:{context.Method.FullName} :Before");
            await _next(context);
            _logger.LogInformation($"LoggingMiddleware:{context.Method.FullName} :End");
        }
    }
}