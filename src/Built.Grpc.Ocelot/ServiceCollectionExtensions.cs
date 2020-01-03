using Built.Grpcc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocelot.DependencyInjection;
using System;

namespace Built.Grpc.Ocelot
{
    public static class ServiceCollectionExtensions
    {
        public static IOcelotBuilder AddGrpcHttpGateway(this IOcelotBuilder builder)
        {
            //var bases = AppDomain.CurrentDomain.BaseDirectory;
            return builder.AddGrpcHttpGateway(new GrpcHttpGatewayConfiguration
            {
                PluginMonitor = true,
                ProtoMonitor = true
            });
        }

        public static IOcelotBuilder AddGrpcHttpGateway(this IOcelotBuilder builder, IConfiguration config)
        {
            var grpcConfig = config.GetSection("GrpcHttpGateway").Get<GrpcHttpGatewayConfiguration>();
            return builder.AddGrpcHttpGateway(grpcConfig);
        }

        public static IOcelotBuilder AddGrpcHttpGateway(this IOcelotBuilder builder, GrpcHttpGatewayConfiguration config)
        {
            builder.Services.AddGrpcHttpGateway(config);
            return builder;
        }

        private static IServiceCollection AddGrpcHttpGateway(this IServiceCollection services, GrpcHttpGatewayConfiguration config)
        {
            services.Configure<GrpcHttpGatewayConfiguration>(c =>
            {
                c.PluginMonitor = config.PluginMonitor;
                c.ProtoMonitor = config.ProtoMonitor;
            });
            Built.Grpcc.ServiceCollectionExtensions.AddServices(services);
            services.TryAddTransient<GrpcRequestBuilder>();
            return services;
        }
    }
}