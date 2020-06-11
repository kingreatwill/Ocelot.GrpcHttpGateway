using Built.Grpc.Pool;
using Built.Grpcc;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Ocelot.Configuration.File;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.Responses;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Built.Grpc.Ocelot
{
    public class OcelotGrpcHttpMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate next;
        private readonly GrpcPool grpcPool;
        private readonly GrpcRequestBuilder grpcRequestBuilder;

        public OcelotGrpcHttpMiddleware(RequestDelegate next,
            GrpcPool grpcPool,
            GrpcRequestBuilder grpcRequestBuilder,
            IOcelotLoggerFactory factory) : base(factory.CreateLogger<OcelotGrpcHttpMiddleware>())
        {
            this.next = next;
            this.grpcPool = grpcPool;
            this.grpcRequestBuilder = grpcRequestBuilder;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            object result = null;
            var errMessage = string.Empty;
            var httpStatusCode = HttpStatusCode.OK;
            var buildRequest = grpcRequestBuilder.BuildRequest(httpContext);
            if (buildRequest.IsError)
            {
                errMessage = "bad request";
                httpStatusCode = HttpStatusCode.BadRequest;
                Logger.LogWarning(errMessage);
            }
            else
            {
                try
                {
                    var channel = grpcPool.GetChannel(new ServiceEndpoint(httpContext.Items.DownstreamRequest().Host, httpContext.Items.DownstreamRequest().Port));
                    var client = new MethodDescriptorClient(channel);
                    result = await client.InvokeAsync(buildRequest.Data.GrpcMethod, buildRequest.Data.Headers, buildRequest.Data.RequestMessage);
                }
                catch (RpcException ex)
                {
                    httpStatusCode = HttpStatusCode.InternalServerError;
                    errMessage = $"rpc exception.";
                    Logger.LogError($"{ex.StatusCode}--{ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    httpStatusCode = HttpStatusCode.ServiceUnavailable;
                    errMessage = $"error in request grpc service.";
                    Logger.LogError($"{errMessage}--{httpContext.Items.DownstreamRequest().ToUri()}", ex);
                }
            }
            OkResponse<GrpcHttpContent> httpResponse;
            if (string.IsNullOrEmpty(errMessage))
            {
                httpResponse = new OkResponse<GrpcHttpContent>(new GrpcHttpContent(result));
            }
            else
            {
                httpResponse = new OkResponse<GrpcHttpContent>(new GrpcHttpContent(errMessage));
            }
            httpContext.Response.ContentType = "application/json";
            httpContext.Items.UpsertDownstreamResponse(new DownstreamResponse(httpResponse.Data, httpStatusCode, httpResponse.Data.Headers, "OcelotGrpcHttpMiddleware"));
            // httpContext.DownstreamResponse = new DownstreamResponse(httpResponse.Data, httpStatusCode, httpResponse.Data.Headers, "OcelotGrpcHttpMiddleware");
        }
    }
}