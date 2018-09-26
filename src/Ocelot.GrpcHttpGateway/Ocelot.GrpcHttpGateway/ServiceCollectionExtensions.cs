using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ocelot.GrpcHttpGateway
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            Built.Grpcc.ServiceCollectionExtensions.AddServices(services);
            services.TryAddTransient<GrpcRequestBuilder>();
            return services;
        }
    }
}