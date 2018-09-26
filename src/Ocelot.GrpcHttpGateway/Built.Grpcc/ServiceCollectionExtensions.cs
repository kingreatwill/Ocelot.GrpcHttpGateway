using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Built.Grpcc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.TryAddSingleton<CodeBuilder>();
            services.TryAddSingleton<CodeGenerater>();
            services.TryAddSingleton<ServiceDescriptor>();
            services.TryAddSingleton<GrpcPool>();
            //services.TryAddSingleton<IGrpcChannelFactory, GrpcChannelFactory>();
            //services.TryAddTransient<IGrpcRequestBuilder, GrpcRequestBuilder>();
            return services;
        }
    }

    /*
     ServiceCollection services = new ServiceCollection();
    services.AddSingleton<MySingleton>();
    services.AddTransient<MyTransient>();
    services.AddScoped<MyScoped>();

    IServiceProvider sp = services.BuildServiceProvider();
     */
}