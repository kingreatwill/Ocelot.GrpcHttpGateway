using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.Orleans.SwaggerGen
{
    public class GrainKeyParmeterFilter : IParameterFilter
    {
        private readonly OrleansSwaggerGenOptions options;
        public GrainKeyParmeterFilter(OrleansSwaggerGenOptions options)
        {
            this.options = options;
        }
        public void Apply(IParameter parameter, ParameterFilterContext context)
        {
            if (this.options.GrainInterfaceGrainKeyAsName.TryGetValue(context.ParameterInfo.Member.DeclaringType, out GrainKeyDescription keyDescription))
            {
                if (context.ParameterInfo.Name == keyDescription.Name)
                    parameter.Description = keyDescription.Description;
            }
        }
    }
}
