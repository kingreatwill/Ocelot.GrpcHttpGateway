using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Built.Grpc.Middleware
{
    public class PolicyMiddleware
    {
        private PipelineDelagate _next;
        private PolicyMiddlewareOptions _options = new PolicyMiddlewareOptions();

        public PolicyMiddleware(PipelineDelagate next)
        {
            _next = next;
        }

        public PolicyMiddleware(PipelineDelagate next, PolicyMiddlewareOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(MiddlewareContext context)
        {
            var _retryAsync = Policy
                  .Handle<Exception>()
                  .RetryAsync(_options.RetryTimes, async (exception, retryCount) =>
                  {
                      await InnerLogger.LogAsync(LoggerLevel.Error, $"-------第{retryCount}次重试!exception:{exception.Message}");
                  });
            // todo:这里设置没有用,还不知道原因;
            var _timeoutAsync = Policy
                .TimeoutAsync(TimeSpan.FromMilliseconds(_options.TimoutMilliseconds), async (ct, ts, tk, exception) =>
                {
                    await InnerLogger.LogAsync(LoggerLevel.Error, $"---------超时.");
                });
            await Policy.WrapAsync(_retryAsync, _timeoutAsync).ExecuteAsync(async () =>
            {
                await _next(context);
            });
        }
    }

    public class PolicyMiddlewareOptions : IOptions<PolicyMiddlewareOptions>
    {
        public PolicyMiddlewareOptions()
        {
            TimoutMilliseconds = 60000;
            RetryTimes = 0;
        }

        public int TimoutMilliseconds { get; set; }
        public int RetryTimes { get; set; }

        PolicyMiddlewareOptions IOptions<PolicyMiddlewareOptions>.Value
        {
            get
            {
                return this;
            }
        }
    }
}