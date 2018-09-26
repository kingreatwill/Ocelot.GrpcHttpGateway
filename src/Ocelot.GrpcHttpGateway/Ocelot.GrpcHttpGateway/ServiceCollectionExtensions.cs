using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocelot.DependencyInjection;

namespace Ocelot.GrpcHttpGateway
{
    public static class ServiceCollectionExtensions
    {
        public static IOcelotBuilder AddGrpcHttpGateway(this IOcelotBuilder builder)
        {
            builder.Services.AddGrpcHttpGateway();
            return builder;
        }

        private static IServiceCollection AddGrpcHttpGateway(this IServiceCollection services)
        {
            Built.Grpcc.ServiceCollectionExtensions.AddServices(services);
            services.TryAddTransient<GrpcRequestBuilder>();
            return services;
        }
    }
}