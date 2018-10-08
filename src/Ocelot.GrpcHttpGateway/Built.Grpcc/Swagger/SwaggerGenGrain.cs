using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.Orleans.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Swashbuckle.Orleans.SwaggerGen
{
    public class SwaggerGenGrain :Grain, ISwaggerGenGrain
    {
        private readonly ISwaggerProvider swaggerProvider;
        private readonly OrleansSwaggerGenOptions options;
        public SwaggerGenGrain(ISwaggerProvider swaggerProvider, IOptions<OrleansSwaggerGenOptions> options)
        {
            this.swaggerProvider = swaggerProvider;
            this.options = options.Value;
        }
        public Task<string> Generator()
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            var swagger = swaggerProvider.GetSwagger(options.DocumentName,options.Host,options.BasePath,options.Schemes.ToArray());

            JsonSerializer _swaggerSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                ContractResolver = new SwaggerContractResolver(jsonSerializerSettings)
            };

            var jsonBuilder = new StringBuilder();
            using (var writer = new StringWriter(jsonBuilder))
            {
                _swaggerSerializer.Serialize(writer, swagger);
            }
            return Task.FromResult( jsonBuilder.ToString());
        }
    }
}
