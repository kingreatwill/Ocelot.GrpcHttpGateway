using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Built.Grpcc.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenServiceCollectionExtensions
    {
        public static IServiceCollection AddOrleansSwaggerGen(
            this IServiceCollection services,
            Action<GrpcSwaggerGenOptions> option, Action<SwaggerGenOptions> swaggerAction = null)
        {
            GrpcSwaggerGenOptions swaggerGenOptions = new GrpcSwaggerGenOptions();
            option.Invoke(swaggerGenOptions);

            services.AddSwaggerGen(opt =>
            {
                opt.ParameterFilter<GrainKeyParmeterFilter>(swaggerGenOptions);
                swaggerAction?.Invoke(opt);
            });
            services.Configure<GrpcSwaggerGenOptions>(option);
            services.AddSingleton<IApiDescriptionGroupCollectionProvider, Built.Grpcc.SwaggerGen.ApiDescriptionGroupCollectionProvider>();
            return services;
        }
    }
}