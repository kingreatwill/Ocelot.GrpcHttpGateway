using Built.Grpcc;
using Built.Grpcc.Utils;
using Microsoft.AspNetCore.Builder;
using Ocelot.Authentication.Middleware;
using Ocelot.Authorisation.Middleware;
using Ocelot.Cache.Middleware;
using Ocelot.Claims.Middleware;
using Ocelot.DownstreamUrlCreator.Middleware;
using Ocelot.Headers.Middleware;
using Ocelot.LoadBalancer.Middleware;
using Ocelot.Middleware;
using Ocelot.Middleware.Pipeline;
using Ocelot.QueryStrings.Middleware;
using Ocelot.RateLimit.Middleware;
using Ocelot.Request.Middleware;
using Ocelot.RequestId.Middleware;
using System;
using System.Threading.Tasks;

namespace Ocelot.GrpcHttpGateway
{
    public static class OcelotGrpcHttpMiddlewareExtensions
    {
        public static OcelotPipelineConfiguration AddGrpcHttpGateway(this OcelotPipelineConfiguration config)
        {
            config.MapWhenOcelotPipeline.Add(builder => builder.AddGrpcHttpGateway(config));
            return config;
        }

        /// <summary>
        ///  <see cref="OcelotPipelineExtensions"/>
        /// </summary>
        private static Func<DownstreamContext, bool> AddGrpcHttpGateway(this IOcelotPipelineBuilder builder, OcelotPipelineConfiguration pipelineConfiguration)
        {
            // Now we have the ds route we can transform headers and stuff?
            builder.UseHttpHeadersTransformationMiddleware();

            // Initialises downstream request
            builder.UseDownstreamRequestInitialiser();

            // We check whether the request is ratelimit, and if there is no continue processing
            builder.UseRateLimiting();

            // This adds or updates the request id (initally we try and set this based on global config in the error handling middleware)
            // If anything was set at global level and we have a different setting at re route level the global stuff will be overwritten
            // This means you can get a scenario where you have a different request id from the first piece of middleware to the request id middleware.
            builder.UseRequestIdMiddleware();

            // Allow pre authentication logic. The idea being people might want to run something custom before what is built in.
            builder.UseIfNotNull(pipelineConfiguration.PreAuthenticationMiddleware);

            // Now we know where the client is going to go we can authenticate them.
            // We allow the ocelot middleware to be overriden by whatever the
            // user wants
            if (pipelineConfiguration.AuthenticationMiddleware == null)
            {
                builder.UseAuthenticationMiddleware();
            }
            else
            {
                builder.Use(pipelineConfiguration.AuthenticationMiddleware);
            }

            // The next thing we do is look at any claims transforms in case this is important for authorisation
            builder.UseClaimsBuilderMiddleware();

            // Allow pre authorisation logic. The idea being people might want to run something custom before what is built in.
            builder.UseIfNotNull(pipelineConfiguration.PreAuthorisationMiddleware);

            // Now we have authenticated and done any claims transformation we
            // can authorise the request
            // We allow the ocelot middleware to be overriden by whatever the
            // user wants
            if (pipelineConfiguration.AuthorisationMiddleware == null)
            {
                builder.UseAuthorisationMiddleware();
            }
            else
            {
                builder.Use(pipelineConfiguration.AuthorisationMiddleware);
            }

            // Now we can run any header transformation logic
            builder.UseHttpRequestHeadersBuilderMiddleware();

            // Allow the user to implement their own query string manipulation logic
            builder.UseIfNotNull(pipelineConfiguration.PreQueryStringBuilderMiddleware);

            // Now we can run any query string transformation logic
            builder.UseQueryStringBuilderMiddleware();
            // Get the load balancer for this request
            builder.UseLoadBalancingMiddleware();

            // This takes the downstream route we retrieved earlier and replaces any placeholders with the variables that should be used
            builder.UseDownstreamUrlCreatorMiddleware();

            // Not sure if this is the best place for this but we use the downstream url
            // as the basis for our cache key.
            builder.UseOutputCacheMiddleware();

            builder.UseGrpcHttpMiddleware();

            //builder.UseHttpRequesterMiddleware();

            return (context) =>
            {
                return context.DownstreamReRoute.DownstreamScheme.Equals("grpc", StringComparison.OrdinalIgnoreCase);
            };
        }

        private static void UseIfNotNull(this IOcelotPipelineBuilder builder,
             Func<DownstreamContext, Func<Task>, Task> middleware)
        {
            if (middleware != null)
            {
                builder.Use(middleware);
            }
        }

        public static IOcelotPipelineBuilder UseGrpcHttpMiddleware(this IOcelotPipelineBuilder builder)
        {
            ServiceLocator.Instance = builder.ApplicationServices;
            //FileConfiguration
            //builder.ApplicationServices.GetService<GrpcPluginFactory>();
            var plugin = ServiceLocator.GetService<GrpcPluginFactory>();
            var proto = ServiceLocator.GetService<GrpcProtoFactory>();
            plugin.InitAsync();
            proto.InitAsync();
            return builder.UseMiddleware<OcelotGrpcHttpMiddleware>();
        }

        //public static IApplicationBuilder UseGrpcHttpMiddleware(this IApplicationBuilder builder)
        //{
        //    ServiceLocator.Instance = builder.ApplicationServices;
        //    return builder.UseMiddleware<OcelotGrpcHttpMiddleware>();
        //}
    }
}