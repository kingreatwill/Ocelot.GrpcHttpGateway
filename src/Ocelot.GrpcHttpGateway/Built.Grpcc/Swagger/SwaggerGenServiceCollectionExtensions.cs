using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.Orleans.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenServiceCollectionExtensions
    {
        public static IServiceCollection AddOrleansSwaggerGen(
            this IServiceCollection services,
            Action<OrleansSwaggerGenOptions> orleansOption, Action<SwaggerGenOptions> swaggerAction = null)
        {
            OrleansSwaggerGenOptions swaggerGenOptions = new OrleansSwaggerGenOptions();
            orleansOption.Invoke(swaggerGenOptions);

            services.AddSwaggerGen(opt =>
            {
                opt.ParameterFilter<GrainKeyParmeterFilter>(swaggerGenOptions);
                swaggerAction?.Invoke(opt);
            });
            services.Configure<OrleansSwaggerGenOptions>(orleansOption);
            services.AddSingleton<IApiDescriptionGroupCollectionProvider, OrleansApiDescriptionGroupCollectionProvider>();
            return services;
        }


    }
}
