using Built.Grpcc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ocelot.Authentication.Middleware;
using Ocelot.Authorisation.Middleware;
using Ocelot.Cache.Middleware;
using Ocelot.Claims.Middleware;
using Ocelot.DownstreamUrlCreator.Middleware;
using Ocelot.Headers.Middleware;
using Ocelot.LoadBalancer.Middleware;
using Ocelot.Middleware;
using Ocelot.QueryStrings.Middleware;
using Ocelot.RateLimit.Middleware;
using Ocelot.Request.Middleware;
using Ocelot.RequestId.Middleware;
using System;
using System.Threading.Tasks;

namespace Built.Grpc.Ocelot
{
    public static class OcelotGrpcHttpMiddlewareExtensions
    {

        public static IApplicationBuilder UseGrpcHttpMiddleware(this IApplicationBuilder builder)
        {
            ServiceLocator.Instance = builder.ApplicationServices;
            var plugin = ServiceLocator.GetService<GrpcPluginFactory>();
            var proto = ServiceLocator.GetService<GrpcProtoFactory>();
            plugin.InitAsync();
            proto.InitAsync();

            return builder.UseMiddleware<OcelotGrpcHttpMiddleware>();
        }
    }
}