using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Built.Grpcc
{
    public class TimeoutMiddlewareOptions : IOptions<TimeoutMiddlewareOptions>
    {
        public TimeoutMiddlewareOptions()
        {
            TimoutMilliseconds = 60000;
        }

        //  public TimeoutMiddlewareOptions Value => throw new System.NotImplementedException();
        public int TimoutMilliseconds { get; set; }

        TimeoutMiddlewareOptions IOptions<TimeoutMiddlewareOptions>.Value
        {
            get
            {
                return this;
            }
        }
    }

    public class TimeoutMiddleware
    {
        private PipelineDelagate _next;
        private readonly ILogger _logger;
        private const string TIMEOUT_KEY = "grpc-timeout";
        private TimeoutMiddlewareOptions _options = new TimeoutMiddlewareOptions();

        public TimeoutMiddleware(PipelineDelagate next, ILogger<CodeBuilder> logger)
        {
            _next = next;
            _logger = logger;
        }

        public TimeoutMiddleware(PipelineDelagate next, ILogger<CodeBuilder> logger, TimeoutMiddlewareOptions options)
        {
            _next = next;
            _logger = logger;
            _options = options;
        }

        public async Task Invoke(MiddlewareContext context)
        {
            if (context.Options.Headers == null)
                context.Options = context.Options.WithHeaders(new Metadata());

            if (!context.Options.Deadline.HasValue)
            {
                context.Options.Headers.Add(TIMEOUT_KEY, $"{_options.TimoutMilliseconds}m");
            }
            await _next(context);
            _logger.LogInformation($"TimeoutMiddleware:{context.Method.FullName} :End");
        }
    }
}