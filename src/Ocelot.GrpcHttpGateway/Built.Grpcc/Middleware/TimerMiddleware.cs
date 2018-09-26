using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Built.Grpcc
{
    public class TimerMiddleware
    {
        private readonly PipelineDelagate _next;
        private readonly ILogger _logger;

        public TimerMiddleware(PipelineDelagate next, ILogger<CodeBuilder> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(MiddlewareContext context)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            await _next(context);
            stopwatch.Stop();
            _logger.LogInformation($"TimerMiddleware:{stopwatch.ElapsedMilliseconds}");
        }
    }
}