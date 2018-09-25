using Built.Grpcc;
using Grpc.Core;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.Responses;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ocelot.GrpcHttpGateway
{
    public class OcelotGrpcHttpMiddleware : OcelotMiddleware
    {
        private readonly OcelotRequestDelegate next;
        private readonly GrpcPool grpcPool;
        private readonly IGrpcRequestBuilder grpcRequestBuilder;

        public OcelotGrpcHttpMiddleware(OcelotRequestDelegate next,
            GrpcPool grpcPool,
            IGrpcRequestBuilder grpcRequestBuilder,
            IOcelotLoggerFactory factory) : base(factory.CreateLogger<OcelotGrpcHttpMiddleware>())
        {
            this.next = next;
            this.grpcPool = grpcPool;
            this.grpcRequestBuilder = grpcRequestBuilder;
        }

        public async Task Invoke(DownstreamContext context)
        {
            string resultMessage = string.Empty;
            var httpStatusCode = HttpStatusCode.OK;
            var buildRequest = grpcRequestBuilder.BuildRequest(context);
            if (buildRequest.IsError)
            {
                resultMessage = "bad request";
                httpStatusCode = HttpStatusCode.BadRequest;
                Logger.LogDebug(resultMessage);
            }
            else
            {
                try
                {
                    //缓存连接应该使用服务发现或执行健康检查，不如会等太久
                    var channel = grpcPool.GetChannel(new ServiceEndpoint(context.DownstreamRequest.Host, context.DownstreamRequest.Port));
                    var client = new GrpcClient(channel);
                    resultMessage = await client.InvokeMethodAsync(buildRequest.Data.GrpcMethod, buildRequest.Data.RequestMessage);
                }
                catch (RpcException ex)
                {
                    httpStatusCode = HttpStatusCode.InternalServerError;
                    resultMessage = $"rpc exception.";
                    Logger.LogError($"{ex.StatusCode}--{ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    httpStatusCode = HttpStatusCode.ServiceUnavailable;
                    resultMessage = $"error in request grpc service.";
                    Logger.LogError($"{resultMessage}--{context.DownstreamRequest.ToUri()}", ex);
                }
            }
            OkResponse<GrpcHttpContent> httpResponse = new OkResponse<GrpcHttpContent>(new GrpcHttpContent(resultMessage));
            context.HttpContext.Response.ContentType = "application/json";
            context.DownstreamResponse = new DownstreamResponse(httpResponse.Data, httpStatusCode, httpResponse.Data.Headers);
        }
    }
}